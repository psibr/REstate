using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Remote.Models;
using REstate.Remote.Services;
using REstate.Schematics;

namespace REstate.Remote
{
    public class GrpcStateEngine<TState, TInput>
        : IRemoteStateEngine<TState, TInput>
    {
        private readonly IStateMachineService _stateMachineService;

        public GrpcStateEngine(IStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService
                .WithHeaders(new Metadata
                {
                    { "Status-Type", typeof(TState).AssemblyQualifiedName },
                    { "Input-Type", typeof(TInput).AssemblyQualifiedName }
                });
        }

        public Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            ISchematic<TState, TInput> schematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
            => CreateMachineAsync(schematic.Copy(), metadata, cancellationToken);

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .CreateMachineFromSchematicAsync(new CreateMachineFromSchematicRequest
                {
                    SchematicBytes = MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance),
                    Metadata = metadata
                });

            return new GrpcStateMachine<TState, TInput>(_stateMachineService, response.MachineId);
        }

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            string schematicName,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .CreateMachineFromStoreAsync(new CreateMachineFromStoreRequest
                {
                    SchematicName = schematicName,
                    Metadata = metadata
                });

            return new GrpcStateMachine<TState, TInput>(_stateMachineService, response.MachineId);
        }

        public Task BulkCreateMachinesAsync(
            ISchematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return BulkCreateMachinesAsync(schematic.Copy(), metadata, cancellationToken);
        }

        public async Task BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .BulkCreateMachineFromSchematicAsync(new BulkCreateMachineFromSchematicRequest
                {
                    SchematicBytes = MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance),
                    Metadata = metadata
                });
        }

        public async Task BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .BulkCreateMachineFromStoreAsync(new BulkCreateMachineFromStoreRequest
                {
                    SchematicName = schematicName,
                    Metadata = metadata
                });
        }

        public async Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .DeleteMachineAsync(new DeleteMachineRequest
                {
                    MachineId = machineId
                });
        }

        public Task<IStateMachine<TState, TInput>> GetMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default(CancellationToken)) =>
                Task.FromResult<IStateMachine<TState, TInput>>(
                    new GrpcStateMachine<TState, TInput>(_stateMachineService, machineId));

        public async Task<ISchematic<TState, TInput>> GetSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .GetSchematicAsync(new GetSchematicRequest
                {
                    SchematicName = schematicName
                });

            return MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                response.SchematicBytes,
                ContractlessStandardResolver.Instance);
        }

        public async Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .StoreSchematicAsync(new StoreSchematicRequest
                {
                    SchematicBytes = MessagePackSerializer.Serialize(
                        schematic,
                        ContractlessStandardResolver.Instance)
                });

            return MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                response.SchematicBytes,
                ContractlessStandardResolver.Instance);
        }

        public Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            ISchematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default(CancellationToken)) 
            => StoreSchematicAsync(schematic.Copy(), cancellationToken);
    }
}