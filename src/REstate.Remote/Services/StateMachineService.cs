using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Engine;
using REstate.Remote.Models;
using REstate.Schematics;

#pragma warning disable 1998

namespace REstate.Remote.Services
{
    public interface IStateMachineService
        : IService<IStateMachineService>
    {
        UnaryResult<SendResponse> SendAsync(SendRequest request);

        UnaryResult<SendResponse> SendWithPayloadAsync(SendWithPayloadRequest request);

        UnaryResult<StoreSchematicResponse> StoreSchematicAsync(StoreSchematicRequest request);

        UnaryResult<GetMachineSchematicResponse> GetMachineSchematicAsync(GetMachineSchematicRequest request);

        UnaryResult<GetSchematicResponse> GetSchematicAsync(GetSchematicRequest request);

        UnaryResult<Nil> DeleteMachineAsync(DeleteMachineRequest request);

        UnaryResult<CreateMachineResponse> CreateMachineFromStoreAsync(CreateMachineFromStoreRequest request);

        UnaryResult<CreateMachineResponse> CreateMachineFromSchematicAsync(CreateMachineFromSchematicRequest request);

        UnaryResult<Nil> BulkCreateMachineFromStoreAsync(BulkCreateMachineFromStoreRequest request);

        UnaryResult<Nil> BulkCreateMachineFromSchematicAsync(BulkCreateMachineFromSchematicRequest request);
    }

    public class StateMachineService
        : ServiceBase<IStateMachineService>
        , IStateMachineService
    {
        private const string StateTypeHeaderKey = "Status-Type";
        private const string InputTypeHeaderKey = "Input-Type";

        #region SendAsync
        public async UnaryResult<SendResponse> SendAsync(SendRequest request)
        {
            (var stateType, var inputType) = GetGenericTupleFromHeaders();

            var input = MessagePackSerializer.NonGeneric.Deserialize(
                inputType,
                request.InputBytes,
                ContractlessStandardResolver.Instance);

            var sendAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(stateType, inputType);

            return await (Task<SendResponse>)sendAsyncMethod.Invoke(this,
                new[]
                {
                    request.MachineId,
                    input,
                    request.CommitTag,
                    Context.CallContext.CancellationToken
                });
        }

        private static async Task<SendResponse> SendAsync<TState, TInput>(string machineId, TInput input, Guid? commitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent.AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);

            Status<TState> newStatus;

            try
            {
                newStatus = commitTag != null
                    ? await machine.SendAsync(input, commitTag.Value, cancellationToken).ConfigureAwait(false)
                    : await machine.SendAsync(input, cancellationToken).ConfigureAwait(false);
            }
            catch (StateConflictException conflictException)
            {
                throw new ReturnStatusException(StatusCode.AlreadyExists, conflictException.Message);
            }

            return new SendResponse
            {
                MachineId = machineId,
                CommitTag = newStatus.CommitTag,
                StateBytes = MessagePackSerializer.Serialize(newStatus.State, ContractlessStandardResolver.Instance)
            };
        }
        #endregion SendAsync

        #region SendWithPayloadAsync
        public async UnaryResult<SendResponse> SendWithPayloadAsync(SendWithPayloadRequest request)
        {
            (var stateType, var inputType) = GetGenericTupleFromHeaders();

            var input = MessagePackSerializer.NonGeneric.Deserialize(
                inputType,
                request.InputBytes,
                ContractlessStandardResolver.Instance);

            var payload = MessagePackSerializer.Typeless.Deserialize(request.PayloadBytes);

            var sendAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(SendWithPayloadAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(stateType, inputType, payload.GetType());

            return await (Task<SendResponse>)sendAsyncMethod.Invoke(this,
                new[]
                {
                    request.MachineId, input, payload, request.CommitTag, Context.CallContext.CancellationToken
                });
        }

        private static async Task<SendResponse> SendWithPayloadAsync<TState, TInput, TPayload>(string machineId, TInput input, TPayload payload, Guid? commitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent.AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);

            Status<TState> newStatus;

            try
            {
                newStatus = commitTag != null
                    ? await machine.SendAsync(input, payload, commitTag.Value, cancellationToken).ConfigureAwait(false)
                    : await machine.SendAsync(input, payload, cancellationToken).ConfigureAwait(false);
            }
            catch (StateConflictException conflictException)
            {
                throw new ReturnStatusException(StatusCode.AlreadyExists, conflictException.Message);
            }

            return new SendResponse
            {
                MachineId = machineId,
                CommitTag = newStatus.CommitTag,
                StateBytes = MessagePackSerializer.Serialize(newStatus.State, ContractlessStandardResolver.Instance)
            };
        }
        #endregion SendWithPayloadAsync

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
                SchematicBytes = MessagePackSerializer.NonGeneric.Serialize(newSchematic.GetType(), newSchematic, ContractlessStandardResolver.Instance)
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
                    .Serialize(schematic.GetType(), schematic, ContractlessStandardResolver.Instance)
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

            return await (Task<CreateMachineResponse>)createMachineFromStoreAsyncMethod
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

        #region BulkCreateMachineFromStoreAsync
        public async UnaryResult<Nil> BulkCreateMachineFromStoreAsync(BulkCreateMachineFromStoreRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var bulkCreateMachineFromStoreAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(BulkCreateMachineFromStoreAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            await (Task)bulkCreateMachineFromStoreAsyncMethod
                .Invoke(this, new object[]
                {
                    request.SchematicName,
                    request.Metadata,
                    Context.CallContext.CancellationToken
                });

            return Nil.Default;
        }

        private static async Task BulkCreateMachineFromStoreAsync<TState, TInput>(string schematicName,
            IEnumerable<IDictionary<string, string>> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            await engine.BulkCreateMachinesAsync(schematicName, metadata, cancellationToken);
        }
        #endregion BulkCreateMachineFromStoreAsync

        #region BulkCreateMachineFromSchematicAsync
        public async UnaryResult<Nil> BulkCreateMachineFromSchematicAsync(BulkCreateMachineFromSchematicRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var schematic = MessagePackSerializer.NonGeneric.Deserialize(
                typeof(Schematic<,>).MakeGenericType(genericTypes),
                request.SchematicBytes,
                ContractlessStandardResolver.Instance);

            var bulkCreateMachineFromSchematicAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(BulkCreateMachineFromSchematicAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            await (Task)bulkCreateMachineFromSchematicAsyncMethod
                .Invoke(this, new []
                {
                    schematic,
                    request.Metadata,
                    Context.CallContext.CancellationToken
                });

            return Nil.Default;
        }

        private static async Task BulkCreateMachineFromSchematicAsync<TState, TInput>(Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            await engine.BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
        }
        #endregion BulkCreateMachineFromSchematicAsync

        #region GetSchematicAsync
        public async UnaryResult<GetSchematicResponse> GetSchematicAsync(GetSchematicRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            var getSchematicAsyncMethod = typeof(StateMachineService)
                .GetMethod(nameof(GetSchematicAsync), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(genericTypes);

            return await (Task<GetSchematicResponse>)getSchematicAsyncMethod
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
                SchematicBytes = MessagePackSerializer.NonGeneric.Serialize(schematic.GetType(), schematic, ContractlessStandardResolver.Instance)
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

            await (Task)deleteMachineAsyncMethod.Invoke(this, new object[]
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
                .Where(header => new[] { StateTypeHeaderKey, InputTypeHeaderKey }
                    .Contains(header.Key, StringComparer.OrdinalIgnoreCase))
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