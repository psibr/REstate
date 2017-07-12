using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using REstate.Configuration;
using REstate.Interop.Models;
#pragma warning disable 1998

namespace REstate.Interop.Services
{
    public interface IStateMachineService
        : IService<IStateMachineService>
    {
        UnaryResult<SendResponse> SendAsync(SendRequest sendRequest);

        UnaryResult<StoreSchematicResponse> StoreSchematicAsync(StoreSchematicRequest storeSchematicRequest);
    }

    public class StateMachineService
        : ServiceBase<IStateMachineService>
        , IStateMachineService
    {
        private const string StateTypeHeaderKey = "state-type";
        private const string InputTypeHeaderKey = "input-type";

        public async UnaryResult<SendResponse> SendAsync(SendRequest sendRequest)
        {
            (var stateType, var inputType) = GetGenericTupleFromHeaders();

            var input = MessagePackSerializer.NonGeneric.Deserialize(
                inputType,
                sendRequest.InputBytes,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);

            var payload = MessagePackSerializer.Typeless.Deserialize(sendRequest.PayloadBytes);

            var sendAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(stateType, inputType, payload.GetType());

            return await (Task<SendResponse>)sendAsyncMethod.Invoke(this,
                new[]
                {
                    sendRequest.MachineId, input, payload, sendRequest.CommitTag, Context.CallContext.CancellationToken
                });
        }

        public async UnaryResult<StoreSchematicResponse> StoreSchematicAsync(StoreSchematicRequest storeSchematicRequest)
        {
            var genericTypes = GetGenericsFromHeaders();

            var schematic = MessagePackSerializer.NonGeneric.Deserialize(
                typeof(Schematic<,>).MakeGenericType(genericTypes),
                storeSchematicRequest.SchematicBytes,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);

            var storeSchematicAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(StoreSchematicAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(schematic.GetType().GenericTypeArguments);

            return await (Task<StoreSchematicResponse>)storeSchematicAsyncMethod
                .Invoke(this, new[]
                {
                    schematic,
                    Context.CallContext.CancellationToken
                });
        }

        private Type[] GetGenericsFromHeaders()
        {
            return Context.CallContext.RequestHeaders
                .Where(header => new[] { StateTypeHeaderKey, InputTypeHeaderKey }.Contains(header.Key))
                .OrderByDescending(header => header.Key)
                .Select(header => Type.GetType(header.Value))
                .ToArray();
        }

        private (Type stateType, Type inputType) GetGenericTupleFromHeaders()
        {
            var array = GetGenericsFromHeaders();

            return (array[0], array[1]);
        }

        private static async Task<StoreSchematicResponse> StoreSchematicAsync<TState, TInput>(Schematic<TState, TInput> schematic, CancellationToken cancellationToken)
        {
            var engine = REstateHost.GetStateEngine<TState, TInput>();

            var newSchematic = await engine.StoreSchematicAsync(schematic, cancellationToken);

            return new StoreSchematicResponse
            {
                SchematicBytes = MessagePackSerializer.Serialize(newSchematic, MessagePack.Resolvers.ContractlessStandardResolver.Instance)
            };
        }

        private static async Task<SendResponse> SendAsync<TState, TInput, TPayload>(string machineId, TInput input, TPayload payload, Guid? commitTag, CancellationToken cancellationToken)
        {
            var engine = REstateHost.GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken);

            var newState = await machine.SendAsync(input, payload, commitTag, cancellationToken);

            return new SendResponse
            {
                MachineId = machineId,
                CommitTag = newState.CommitTag,
                StateBytes = MessagePackSerializer.Serialize(newState.Value, MessagePack.Resolvers.ContractlessStandardResolver.Instance)
            };
        }
    }
}