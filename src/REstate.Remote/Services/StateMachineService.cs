using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Remote.Models;
using REstate.Schematics;

#pragma warning disable 1998

namespace REstate.Remote.Services
{
    public interface IStateMachineService
        : IService<IStateMachineService>
    {
        UnaryResult<SendResponse> SendAsync(SendRequest request);

        UnaryResult<StoreSchematicResponse> StoreSchematicAsync(StoreSchematicRequest request);

        UnaryResult<GetMachineSchematicResponse> GetMachineSchematicAsync(GetMachineSchematicRequest request);

        UnaryResult<GetSchematicResponse> GetSchematicAsync(GetSchematicRequest request);

        UnaryResult<Nil> DeleteMachineAsync(DeleteMachineRequest request);

        UnaryResult<CreateMachineResponse> CreateMachineFromStoreAsync(CreateMachineFromStoreRequest request);

        UnaryResult<CreateMachineResponse> CreateMachineFromSchematicAsync(CreateMachineFromSchematicRequest request);
    }

    public class StateMachineService
        : ServiceBase<IStateMachineService>
        , IStateMachineService
    {
        private const string StateTypeHeaderKey = "State-Type";
        private const string InputTypeHeaderKey = "Input-Type";

        #region SendAsync
        public async UnaryResult<SendResponse> SendAsync(SendRequest request)
        {
            (var stateType, var inputType) = GetGenericTupleFromHeaders();

            var input = MessagePackSerializer.NonGeneric.Deserialize(
                inputType,
                request.InputBytes,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);

            var payload = MessagePackSerializer.Typeless.Deserialize(request.PayloadBytes);

            var sendAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(stateType, inputType, payload.GetType());

            return await (Task<SendResponse>)sendAsyncMethod.Invoke(this,
                new[]
                {
                    request.MachineId, input, payload, request.CommitTag, Context.CallContext.CancellationToken
                });
        }

        private static async Task<SendResponse> SendAsync<TState, TInput, TPayload>(string machineId, TInput input, TPayload payload, Guid? commitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent.AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);

            var newState = await machine.SendAsync(input, payload, commitTag, cancellationToken).ConfigureAwait(false);

            return new SendResponse
            {
                MachineId = machineId,
                CommitTag = newState.CommitTag,
                StateBytes = MessagePackSerializer.Serialize(newState.Value, MessagePack.Resolvers.ContractlessStandardResolver.Instance)
            };
        }
        #endregion SendAsync

        #region StoreSchematicAsync
        public async UnaryResult<StoreSchematicResponse> StoreSchematicAsync(StoreSchematicRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var schematic = MessagePackSerializer.NonGeneric.Deserialize(
                typeof(Schematic<,>).MakeGenericType(genericTypes),
                request.SchematicBytes,
                ContractlessStandardResolver.Instance);

            var storeSchematicAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(StoreSchematicAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            return await (Task<StoreSchematicResponse>)storeSchematicAsyncMethod
                .Invoke(this, new[]
                {
                    schematic,
                    Context.CallContext.CancellationToken
                });
        }

        private static async Task<StoreSchematicResponse> StoreSchematicAsync<TState, TInput>(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent.AsLocal()
                .GetStateEngine<TState, TInput>();

            var newSchematic = await engine.StoreSchematicAsync(schematic, cancellationToken).ConfigureAwait(false);

            return new StoreSchematicResponse
            {
                SchematicBytes = MessagePackSerializer.NonGeneric.Serialize(newSchematic.GetType(), newSchematic, MessagePack.Resolvers.ContractlessStandardResolver.Instance)
            };
        }
        #endregion StoreSchematicAsync

        #region GetMachineSchematicAsync
        public async UnaryResult<GetMachineSchematicResponse> GetMachineSchematicAsync(GetMachineSchematicRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var getMachineAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(GetMachineSchematicAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            return await (Task<GetMachineSchematicResponse>)getMachineAsyncMethod
                .Invoke(this, new object[]
                {
                    request.MachineId,
                    Context.CallContext.CancellationToken
                });
        }

        private static async Task<GetMachineSchematicResponse> GetMachineSchematicAsync<TState, TInput>(string machineId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);

            var schematic = await machine.GetSchematicAsync(cancellationToken).ConfigureAwait(false);

            return new GetMachineSchematicResponse
            {
                MachineId = machine.MachineId,
                SchematicBytes = MessagePackSerializer.NonGeneric
                    .Serialize(machine.Schematic.GetType(), machine.Schematic, ContractlessStandardResolver.Instance)
            };
        }
        #endregion GetMachineSchematicAsync

        #region CreateMachineFromStoreAsync
        public async UnaryResult<CreateMachineResponse> CreateMachineFromStoreAsync(CreateMachineFromStoreRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var createMachineFromStoreAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(CreateMachineFromStoreAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            return await(Task<CreateMachineResponse>) createMachineFromStoreAsyncMethod
                .Invoke(this, new object[]
                {
                    request.SchematicName,
                    request.Metadata,
                    Context.CallContext.CancellationToken
                });
        }

        private static async Task<CreateMachineResponse> CreateMachineFromStoreAsync<TState, TInput>(string schematicName,
            IDictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.CreateMachineAsync(schematicName, metadata, cancellationToken);

            return new CreateMachineResponse
            {
                MachineId = machine.MachineId
            };
        }
        #endregion CreateMachineFromStoreAsync

        #region CreateMachineFromSchematicAsync
        public async UnaryResult<CreateMachineResponse> CreateMachineFromSchematicAsync(CreateMachineFromSchematicRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var schematic = MessagePackSerializer.NonGeneric.Deserialize(
                typeof(Schematic<,>).MakeGenericType(genericTypes),
                request.SchematicBytes,
                ContractlessStandardResolver.Instance);

            var createMachineFromStoreAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(CreateMachineFromSchematicAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            return await (Task<CreateMachineResponse>)createMachineFromStoreAsyncMethod
                .Invoke(this, new[]
                {
                    schematic,
                    request.Metadata,
                    Context.CallContext.CancellationToken
                });
        }

        private static async Task<CreateMachineResponse> CreateMachineFromSchematicAsync<TState, TInput>(Schematic<TState, TInput> schematic,
            IDictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.CreateMachineAsync(schematic, metadata, cancellationToken);

            return new CreateMachineResponse
            {
                MachineId = machine.MachineId
            };
        }
        #endregion CreateMachineFromSchematicAsync

        #region GetSchematicAsync
        public async UnaryResult<GetSchematicResponse> GetSchematicAsync(GetSchematicRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var getSchematicAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(GetSchematicAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            return await (Task<GetSchematicResponse>) getSchematicAsyncMethod
                .Invoke(this, new object[]
                {
                    request.SchematicName,
                    Context.CallContext.CancellationToken
                });
        }

        private static async Task<GetSchematicResponse> GetSchematicAsync<TState, TInput>(string schematicName, CancellationToken cancellationToken)
        {
            var stateEngine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var schematic = await stateEngine.GetSchematicAsync(schematicName, cancellationToken).ConfigureAwait(false);

            return new GetSchematicResponse
            {
                SchematicBytes = MessagePackSerializer.NonGeneric.Serialize(schematic.GetType(), schematic, MessagePack.Resolvers.ContractlessStandardResolver.Instance)
            };
        }
        #endregion GetSchematicAsync

        #region DeleteMachineAsync
        public async UnaryResult<Nil> DeleteMachineAsync(DeleteMachineRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var deleteMachineAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(DeleteMachineAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            await (Task) deleteMachineAsyncMethod.Invoke(this, new object[]
            {
                request.MachineId,
                Context.CallContext.CancellationToken
            });

            return Nil.Default;
        }

        private static async Task DeleteMachineAsync<TState, TInput>(string machineId, CancellationToken cancellationToken)
        {
            var stateEngine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            await stateEngine.DeleteMachineAsync(machineId, cancellationToken).ConfigureAwait(false);
        }
        #endregion DeleteMachineAsync

        private Type[] GetGenericsFromHeaders()
        {
            return Context.CallContext.RequestHeaders
                .Where(header => new[] { StateTypeHeaderKey, InputTypeHeaderKey }.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(header => header.Key)
                .Select(header => Type.GetType(header.Value))
                .ToArray();
        }

        private (Type stateType, Type inputType) GetGenericTupleFromHeaders()
        {
            var array = GetGenericsFromHeaders();

            return (array[0], array[1]);
        }
    }
}