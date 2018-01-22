using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Remote.Models;
using REstate.Schematics;

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
    using CreateMachineFromStoreAsyncDelegate = Func<string, string, IDictionary<string, string>, CancellationToken, Task<CreateMachineResponse>>;
    using CreateMachineFromSchematicAsyncDelegate = Func<object, string, IDictionary<string, string>, CancellationToken, Task<CreateMachineResponse>>;
    using BulkCreateMachineFromStoreAsyncDelegate = Func<string, IEnumerable<IDictionary<string, string>>, CancellationToken, Task<BulkCreateMachineResponse>>;
    using BulkCreateMachineFromSchematicAsyncDelegate = Func<object, IEnumerable<IDictionary<string, string>>, CancellationToken, Task<BulkCreateMachineResponse>>;
    using GetSchematicAsyncDelegate = Func<string, CancellationToken, Task<GetSchematicResponse>>;
    using DeleteMachineAsyncDelegate = Func<string, CancellationToken, Task>;

    public class StateMachineService
        : ServiceBase<IStateMachineService>
        , IStateMachineService
    {
        private static readonly ConcurrentDictionary<MethodKey, Delegate> SharedDelegateCache =
            new ConcurrentDictionary<MethodKey, Delegate>();

        private const string StateTypeHeaderKey = "Status-Type";
        private const string InputTypeHeaderKey = "Input-Type";
        private const Type NoPayloadType = null;

        private ConcurrentDictionary<MethodKey, Delegate> DelegateCache { get; }
        private IStateMachineServiceLocalAdapter LocalAdapter { get; }

        public StateMachineService()
            : this(SharedDelegateCache, new StateMachineServiceLocalAdapter())
        {
        }

        /// <summary>
        /// This is primarily a test constructor. 
        /// <para />
        /// It allow us to test the expression trees without invoking any code 
        /// and without caching delegates to the static, which interferes with scenario tests.
        /// </summary>
        /// <param name="delegateCache"></param>
        /// <param name="localAdapter"></param>
        internal StateMachineService(ConcurrentDictionary<MethodKey, Delegate> delegateCache,  IStateMachineServiceLocalAdapter localAdapter)
        {
            DelegateCache = delegateCache;
            LocalAdapter = localAdapter;
        }

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
                                    instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
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
                GetCallCancellationToken());
        }

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
                                    instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
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
                    GetCallCancellationToken());
        }

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
                                    instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
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

            return await storeSchematicAsync(schematic, GetCallCancellationToken());
        }

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
                                    instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
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

            return await getMachineSchematicAsync(request.MachineId, GetCallCancellationToken());
        }

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
                                instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
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

            return await getMachineMetadataAsync(request.MachineId, GetCallCancellationToken());
        }

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
                        var machineIdParameter = Expression.Parameter(typeof(string), "machineId");
                        var metadataParameter = Expression.Parameter(typeof(IDictionary<string, string>), "metadata");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<CreateMachineFromStoreAsyncDelegate>(
                            body: Expression.Call(
                                instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
                                methodName: nameof(CreateMachineFromStoreAsync),
                                typeArguments: genericTypes,
                                arguments: new Expression[]
                                {
                                    schematicNameParameter,
                                    machineIdParameter,
                                    metadataParameter,
                                    cancellationTokenParameter
                                }),
                            parameters: new ParameterExpression[]
                            {
                                schematicNameParameter,
                                machineIdParameter,
                                metadataParameter,
                                cancellationTokenParameter
                            })
                            .Compile();
                    });

            return await createMachineFromStoreAsync(
                request.SchematicName, 
                request.MachineId,
                request.Metadata, 
                GetCallCancellationToken());
        }
        
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
                        var machineIdParameter = Expression.Parameter(typeof(string), "machineId");
                        var metadataParameter = Expression.Parameter(typeof(IDictionary<string, string>), "metadata");
                        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

                        return Expression.Lambda<CreateMachineFromSchematicAsyncDelegate>(
                            body: Expression.Call(
                                instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
                                methodName: nameof(CreateMachineFromSchematicAsync),
                                typeArguments: genericTypes,
                                arguments: new Expression[]
                                {
                                    Expression.Convert(schematicParameter, schematicType),
                                    machineIdParameter,
                                    metadataParameter,
                                    cancellationTokenParameter
                                }),
                            parameters: new ParameterExpression[]
                            {
                                schematicParameter,
                                machineIdParameter,
                                metadataParameter,
                                cancellationTokenParameter
                            })
                            .Compile();
                    });

            return await createMachineFromSchematicAsync(
                schematic, 
                request.MachineId,
                request.Metadata, 
                GetCallCancellationToken());
        }
        
        public async UnaryResult<BulkCreateMachineResponse> BulkCreateMachineFromStoreAsync(BulkCreateMachineFromStoreRequest request)
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
                                    instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
                                    methodName: nameof(BulkCreateMachineFromStoreAsync),
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

            return await bulkCreateMachineFromStoreAsync(request.SchematicName, request.Metadata, GetCallCancellationToken());
        }
        
        public async UnaryResult<BulkCreateMachineResponse> BulkCreateMachineFromSchematicAsync(BulkCreateMachineFromSchematicRequest request)
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
                                instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
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

            return await bulkCreateMachineFromSchematicAsync(schematic, request.Metadata, GetCallCancellationToken());
        }
        
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
                                instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
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

            return await getSchematicAsync(request.SchematicName, GetCallCancellationToken());
        }
        
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
                                instance: Expression.Constant(LocalAdapter, LocalAdapter.GetType()),
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

            await deleteMachineAsync(request.MachineId, GetCallCancellationToken());

            return Nil.Default;
        }

        internal virtual Type[] GetGenericsFromHeaders()
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

        internal virtual CancellationToken GetCallCancellationToken()
        {
            return Context.CallContext.CancellationToken;
        }
    }
}