using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Server;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Engine;
using REstate.Remote.Models;
using REstate.Schematics;

#pragma warning disable 1998

namespace REstate.Remote.Services
{
    // method name, state type, input type, payload type
    using MethodKey = ValueTuple<string, Type, Type, Type>;

    // non generic delegates for generic method calls
    using SendAsyncDelegate = Func<string, object, Guid?, CancellationToken, Task<SendResponse>>;
    using SendWithPayloadAsyncDelegate = Func<string, object, object, Guid?, CancellationToken, Task<SendResponse>>;
    using StoreSchematicAsyncDelegate = Func<object, CancellationToken, Task<StoreSchematicResponse>>;
    using GetMachineSchematicAsyncDelegate = Func<string, CancellationToken, Task<GetMachineSchematicResponse>>;
    using GetMachineMetadataAsyncDelegate = Func<string, CancellationToken, Task<GetMachineMetadataResponse>>;
    using CreateMachineFromStoreAsyncDelegate = Func<string, IDictionary<string, string>, CancellationToken, Task<CreateMachineResponse>>;
    using CreateMachineFromSchematicAsyncDelegate = Func<object, IDictionary<string, string>, CancellationToken, Task<CreateMachineResponse>>;
    using BulkCreateMachineFromStoreAsyncDelegate = Func<string, IEnumerable<IDictionary<string, string>>, CancellationToken, Task>;
    using BulkCreateMachineFromSchematicAsyncDelegate = Func<object, IEnumerable<IDictionary<string, string>>, CancellationToken, Task>;
    using GetSchematicAsyncDelegate = Func<string, CancellationToken, Task<GetSchematicResponse>>;
    using DeleteMachineAsyncDelegate = Func<string, CancellationToken, Task>;

    public interface IStateMachineServiceClient
    {
        IStateMachineService Create();
    }

    public interface IStateMachineService
        : IService<IStateMachineService>
    {
        UnaryResult<SendResponse> SendAsync(SendRequest request);

        UnaryResult<SendResponse> SendWithPayloadAsync(SendWithPayloadRequest request);

        UnaryResult<StoreSchematicResponse> StoreSchematicAsync(StoreSchematicRequest request);

        UnaryResult<GetMachineSchematicResponse> GetMachineSchematicAsync(GetMachineSchematicRequest request);

        UnaryResult<GetMachineMetadataResponse> GetMachineMetadataAsync(GetMachineMetadataRequest request);

        UnaryResult<GetSchematicResponse> GetSchematicAsync(GetSchematicRequest request);

        UnaryResult<Nil> DeleteMachineAsync(DeleteMachineRequest request);

        UnaryResult<CreateMachineResponse> CreateMachineFromStoreAsync(CreateMachineFromStoreRequest request);

        UnaryResult<CreateMachineResponse> CreateMachineFromSchematicAsync(CreateMachineFromSchematicRequest request);

        UnaryResult<Nil> BulkCreateMachineFromStoreAsync(BulkCreateMachineFromStoreRequest request);

        UnaryResult<Nil> BulkCreateMachineFromSchematicAsync(BulkCreateMachineFromSchematicRequest request);
    }

    public class StateMachineServiceClient
        : IStateMachineServiceClient
    {
        private readonly GrpcHostOptions _gRpcHostOptions;

        public StateMachineServiceClient(GrpcHostOptions gRpcHostOptions)
        {
            _gRpcHostOptions = gRpcHostOptions;
        }

        public IStateMachineService Create() =>
            MagicOnionClient.Create<IStateMachineService>(_gRpcHostOptions.Channel);
    }

    public class StateMachineService
        : ServiceBase<IStateMachineService>
        , IStateMachineService
    {
        private const string StateTypeHeaderKey = "Status-Type";
        private const string InputTypeHeaderKey = "Input-Type";
        private const Type NoPayloadType = null;

        private static readonly ConcurrentDictionary<MethodKey, Delegate> DelegateCache =
            new ConcurrentDictionary<MethodKey, Delegate>();

        #region SendAsync
        public async UnaryResult<SendResponse> SendAsync(SendRequest request)
        {
            (var stateType, var inputType) = GetGenericTupleFromHeaders();

            var input = MessagePackSerializer.NonGeneric.Deserialize(
                inputType,
                request.InputBytes,
                ContractlessStandardResolver.Instance);

            var sendAsync = (SendAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(SendAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var machineIdParameter = Expression.Parameter(typeof(string), "machineId");
                        var inputParameter = Expression.Parameter(typeof(object), "input");
                        var commitTagParameter = Expression.Parameter(typeof(Guid?), "commitTag");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<SendAsyncDelegate>(
                                Expression.Call(
                                    type: typeof(StateMachineService),
                                    methodName: nameof(SendAsync),
                                    typeArguments: new[] { stateType, inputType },
                                    arguments: new Expression[]
                                    {
                                        machineIdParameter,
                                        Expression.Convert(inputParameter, inputType),
                                        commitTagParameter,
                                        cancellationTokenParameter
                                    }),
                                false,
                                machineIdParameter,
                                inputParameter,
                                commitTagParameter,
                                cancellationTokenParameter)
                            .Compile();
                    });

            return await sendAsync(
                request.MachineId,
                input,
                request.CommitTag,
                Context.CallContext.CancellationToken);
        }

        private static async Task<SendResponse> SendAsync<TState, TInput>(
            string machineId,
            TInput input,
            Guid? commitTag,
            CancellationToken cancellationToken = default)
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

            var payloadType = payload.GetType();
            var sendWithPayloadAsync = (SendWithPayloadAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(SendWithPayloadAsync), stateType, inputType, payloadType),
                    valueFactory: tuple =>
                    {
                        var machineIdParameter = Expression.Parameter(typeof(string), "machineId");
                        var inputParameter = Expression.Parameter(typeof(object), "input");
                        var payloadParameter = Expression.Parameter(typeof(object), "payload");
                        var commitTagParameter = Expression.Parameter(typeof(Guid?), "commitTag");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<SendWithPayloadAsyncDelegate>(
                                Expression.Call(
                                    type: typeof(StateMachineService),
                                    methodName: nameof(SendWithPayloadAsync),
                                    typeArguments: new[] { stateType, inputType, payloadType },
                                    arguments: new Expression[]
                                    {
                                            machineIdParameter,
                                            Expression.Convert(inputParameter, inputType),
                                            Expression.Convert(payloadParameter, payloadType),
                                            commitTagParameter,
                                            cancellationTokenParameter
                                    }),
                                false,
                                machineIdParameter,
                                inputParameter,
                                payloadParameter,
                                commitTagParameter,
                                cancellationTokenParameter)
                            .Compile();
                    });

            return await sendWithPayloadAsync(
                    request.MachineId,
                    input,
                    payload,
                    request.CommitTag,
                    Context.CallContext.CancellationToken);
        }

        private static async Task<SendResponse> SendWithPayloadAsync<TState, TInput, TPayload>(string machineId, TInput input, TPayload payload, Guid? commitTag, CancellationToken cancellationToken = default)
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
                StateBytes = MessagePackSerializer.Serialize(newStatus.State, ContractlessStandardResolver.Instance),
                UpdatedTime = newStatus.UpdatedTime
            };
        }
        #endregion SendWithPayloadAsync

        #region StoreSchematicAsync
        public async UnaryResult<StoreSchematicResponse> StoreSchematicAsync(StoreSchematicRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var schematicType = typeof(Schematic<,>).MakeGenericType(genericTypes);

            var schematic = MessagePackSerializer.NonGeneric.Deserialize(
                schematicType,
                request.SchematicBytes,
                ContractlessStandardResolver.Instance);

            var storeSchematicAsync = (StoreSchematicAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(StoreSchematicAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var schematicParameter = Expression.Parameter(typeof(object), "schematic");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<StoreSchematicAsyncDelegate>(
                                body: Expression.Call(
                                    type: typeof(StateMachineService),
                                    methodName: nameof(StoreSchematicAsync),
                                    typeArguments: genericTypes,
                                    arguments: new Expression[]
                                    {
                                        Expression.Convert(schematicParameter, schematicType),
                                        cancellationTokenParameter
                                    }),
                                tailCall: false,
                                parameters: new ParameterExpression[]
                                {
                                    schematicParameter,
                                    cancellationTokenParameter
                                })
                                .Compile();
                    });

            return await storeSchematicAsync(schematic, Context.CallContext.CancellationToken);
        }

        private static async Task<StoreSchematicResponse> StoreSchematicAsync<TState, TInput>(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default)
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

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var getMachineSchematicAsync = (GetMachineSchematicAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(GetMachineSchematicAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var machineIdParameter = Expression.Parameter(typeof(string), "machineId");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<GetMachineSchematicAsyncDelegate>(
                                body: Expression.Call(
                                    type: typeof(StateMachineService),
                                    methodName: nameof(GetMachineSchematicAsync),
                                    typeArguments: genericTypes,
                                    arguments: new Expression[]
                                    {
                                        machineIdParameter,
                                        cancellationTokenParameter
                                    }),
                                tailCall: false,
                                parameters: new ParameterExpression[]
                                {
                                    machineIdParameter,
                                    cancellationTokenParameter
                                })
                                .Compile();
                    });

            return await getMachineSchematicAsync(request.MachineId, Context.CallContext.CancellationToken);
        }

        private static async Task<GetMachineSchematicResponse> GetMachineSchematicAsync<TState, TInput>(string machineId, CancellationToken cancellationToken = default)
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

        #region GetMachineMetadataAsync
        public async UnaryResult<GetMachineMetadataResponse> GetMachineMetadataAsync(GetMachineMetadataRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var getMachineMetadataAsync = (GetMachineMetadataAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(GetMachineMetadataAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var machineIdParameter = Expression.Parameter(typeof(string), "machineId");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<GetMachineMetadataAsyncDelegate>(
                            body: Expression.Call(
                                type: typeof(StateMachineService),
                                methodName: nameof(GetMachineMetadataAsync),
                                typeArguments: genericTypes,
                                arguments: new Expression[]
                                {
                                    machineIdParameter,
                                    cancellationTokenParameter
                                }),
                            parameters: new ParameterExpression[]
                            {
                                machineIdParameter,
                                cancellationTokenParameter
                            })
                            .Compile();
                    });

            return await getMachineMetadataAsync(request.MachineId, Context.CallContext.CancellationToken);
        }

        private static async Task<GetMachineMetadataResponse> GetMachineMetadataAsync<TState, TInput>(string machineId, CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);

            var metadata = await machine.GetMetadataAsync(cancellationToken).ConfigureAwait(false);

            return new GetMachineMetadataResponse
            {
                MachineId = machine.MachineId,
                Metadata = (IDictionary<string, string>)metadata
            };
        }
        #endregion GetMachineMetadataAsync

        #region CreateMachineFromStoreAsync
        public async UnaryResult<CreateMachineResponse> CreateMachineFromStoreAsync(CreateMachineFromStoreRequest request)
        {
            var genericTypes = GetGenericsFromHeaders();

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var createMachineFromStoreAsync = (CreateMachineFromStoreAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(CreateMachineFromStoreAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var schematicNameParameter = Expression.Parameter(typeof(string), "schematicName");
                        var metadataParameter = Expression.Parameter(typeof(IDictionary<string, string>), "metadata");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<CreateMachineFromStoreAsyncDelegate>(
                            body: Expression.Call(
                                type: typeof(StateMachineService),
                                methodName: nameof(CreateMachineFromStoreAsync),
                                typeArguments: genericTypes,
                                arguments: new Expression[]
                                {
                                    schematicNameParameter,
                                    metadataParameter,
                                    cancellationTokenParameter
                                }),
                            parameters: new ParameterExpression[]
                            {
                                schematicNameParameter,
                                metadataParameter,
                                cancellationTokenParameter
                            })
                            .Compile();
                    });

            return await createMachineFromStoreAsync(
                request.SchematicName, 
                request.Metadata, 
                Context.CallContext.CancellationToken);
        }

        private static async Task<CreateMachineResponse> CreateMachineFromStoreAsync<TState, TInput>(string schematicName,
            IDictionary<string, string> metadata, CancellationToken cancellationToken = default)
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

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var schematicType = typeof(Schematic<,>).MakeGenericType(genericTypes);

            var schematic = MessagePackSerializer.NonGeneric.Deserialize(
                schematicType,
                request.SchematicBytes,
                ContractlessStandardResolver.Instance);

            var createMachineFromSchematicAsync = (CreateMachineFromSchematicAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(CreateMachineFromSchematicAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var schematicParameter = Expression.Parameter(typeof(object), "schematic");
                        var metadataParameter = Expression.Parameter(typeof(IDictionary<string, string>), "metadata");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<CreateMachineFromSchematicAsyncDelegate>(
                            body: Expression.Call(
                                type: typeof(StateMachineService),
                                methodName: nameof(CreateMachineFromSchematicAsync),
                                typeArguments: genericTypes,
                                arguments: new Expression[]
                                {
                                    Expression.Convert(schematicParameter, schematicType),
                                    metadataParameter,
                                    cancellationTokenParameter
                                }),
                            parameters: new ParameterExpression[]
                            {
                                schematicParameter,
                                metadataParameter,
                                cancellationTokenParameter
                            })
                            .Compile();
                    });

            return await createMachineFromSchematicAsync(
                schematic, 
                request.Metadata, 
                Context.CallContext.CancellationToken);
        }

        private static async Task<CreateMachineResponse> CreateMachineFromSchematicAsync<TState, TInput>(
            Schematic<TState, TInput> schematic,
            IDictionary<string, string> metadata,
            CancellationToken cancellationToken = default)
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

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var bulkCreateMachineFromStoreAsync = (BulkCreateMachineFromStoreAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(BulkCreateMachineFromStoreAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                        {
                            var schematicNameParameter = Expression.Parameter(typeof(string), "schematicName");
                            var metadataParameter = Expression.Parameter(typeof(IEnumerable<IDictionary<string, string>>), "metadata");
                            var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                            return Expression.Lambda<BulkCreateMachineFromStoreAsyncDelegate>(
                                body: Expression.Call(
                                    type: typeof(StateMachineService),
                                    methodName: nameof(BulkCreateMachineFromStoreAsync),
                                    typeArguments: genericTypes,
                                    arguments: new Expression[]
                                    {
                                        schematicNameParameter,
                                        metadataParameter,
                                        cancellationTokenParameter
                                    }))
                                    .Compile();
                        });

            await bulkCreateMachineFromStoreAsync(request.SchematicName, request.Metadata, Context.CallContext.CancellationToken);

            return Nil.Default;
        }

        private static async Task BulkCreateMachineFromStoreAsync<TState, TInput>(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
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

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var schematicType = typeof(Schematic<,>).MakeGenericType(genericTypes);

            var schematic = MessagePackSerializer.NonGeneric.Deserialize(
                schematicType,
                request.SchematicBytes,
                ContractlessStandardResolver.Instance);

            var bulkCreateMachineFromSchematicAsync = (BulkCreateMachineFromSchematicAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(BulkCreateMachineFromSchematicAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var schematicParameter = Expression.Parameter(typeof(object), "schematic");
                        var metadataParameter = Expression.Parameter(typeof(IEnumerable<IDictionary<string, string>>), "metadata");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<BulkCreateMachineFromSchematicAsyncDelegate>(
                            body: Expression.Call(
                                type: typeof(StateMachineService),
                                methodName: nameof(BulkCreateMachineFromSchematicAsync),
                                typeArguments: genericTypes,
                                arguments: new Expression[]
                                {
                                    Expression.Convert(schematicParameter, schematicType),
                                    metadataParameter,
                                    cancellationTokenParameter
                                }),
                            parameters: new ParameterExpression[] {
                                schematicParameter,
                                metadataParameter,
                                cancellationTokenParameter
                            })
                            .Compile();
                    });

            await bulkCreateMachineFromSchematicAsync(schematic, request.Metadata, Context.CallContext.CancellationToken);

            return Nil.Default;
        }

        private static async Task BulkCreateMachineFromSchematicAsync<TState, TInput>(
            Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata, 
            CancellationToken cancellationToken = default)
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

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var getSchematicAsync = (GetSchematicAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(GetSchematicAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var schematicNameParameter = Expression.Parameter(typeof(string), "schematicName");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<GetSchematicAsyncDelegate>(
                            body: Expression.Call(
                                type: typeof(StateMachineService),
                                methodName: nameof(GetSchematicAsync),
                                typeArguments: genericTypes,
                                arguments: new Expression[]
                                {
                                    schematicNameParameter,
                                    cancellationTokenParameter
                                }),
                            parameters: new ParameterExpression[]
                            {
                                schematicNameParameter,
                                cancellationTokenParameter
                            })
                            .Compile();
                    });

            return await getSchematicAsync(request.SchematicName, Context.CallContext.CancellationToken);
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

            (var stateType, var inputType) = GetGenericTupleFromHeaderArray(genericTypes);

            var deleteMachineAsync = (DeleteMachineAsyncDelegate)DelegateCache
                .GetOrAdd(
                    key: (nameof(DeleteMachineAsync), stateType, inputType, NoPayloadType),
                    valueFactory: tuple =>
                    {
                        var machineIdParameter = Expression.Parameter(typeof(string), "machineId");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<DeleteMachineAsyncDelegate>(
                            body: Expression.Call(
                                type: typeof(StateMachineService),
                                methodName: nameof(DeleteMachineAsync),
                                typeArguments: genericTypes,
                                arguments: new Expression[]
                                {
                                    machineIdParameter,
                                    cancellationTokenParameter
                                }),
                            parameters: new ParameterExpression[]
                            {
                                machineIdParameter,
                                cancellationTokenParameter
                            })
                            .Compile();
                    });

            await deleteMachineAsync(request.MachineId, Context.CallContext.CancellationToken);

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

        private (Type stateType, Type inputType) GetGenericTupleFromHeaderArray(Type[] types)
        {
            return (types[0], types[1]);
        }
    }
}