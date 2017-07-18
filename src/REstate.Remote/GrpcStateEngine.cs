using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MessagePack;
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
                    { "state-type", typeof(TState).FullName },
                    { "input-type", typeof(TInput).FullName }
                });
        }

        public Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            ISchematic<TState, TInput> schematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
            => CreateMachineAsync(schematic.Copy(), metadata, cancellationToken);

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(Schematic<TState, TInput> schematic, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService.CreateMachineFromSchematicAsync(new CreateMachineFromSchematicRequest
            {
                SchematicBytes = MessagePackSerializer.Serialize(schematic, MessagePack.Resolvers.ContractlessStandardResolver.Instance),
                Metadata = metadata
            });

            return new GrpcStateMachine<TState, TInput>(_stateMachineService, null, response.MachineId);
        }

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(string schematicName, IDictionary<string, string> metadata = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService.CreateMachineFromStoreAsync(new CreateMachineFromStoreRequest
            {
                SchematicName = schematicName,
                Metadata = metadata
            });

            return new GrpcStateMachine<TState, TInput>(_stateMachineService, null, response.MachineId);
        }

        public async Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _stateMachineService.DeleteMachineAsync(new DeleteMachineRequest
            {
                MachineId = machineId
            });
        }

        public async Task<IStateMachine<TState, TInput>> GetMachineAsync(string machineId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService.GetMachineAsync(new GetMachineRequest
            {
                MachineId = machineId
            });

            var schematic = MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(response.SchematicBytes,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);

            return new GrpcStateMachine<TState, TInput>(_stateMachineService, schematic, response.MachineId);
        }

        public async Task<ISchematic<TState, TInput>> GetSchematicAsync(string schematicName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService.GetSchematicAsync(new GetSchematicRequest
            {
                SchematicName = schematicName
            });

            return MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(response.SchematicBytes,
                MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        public async Task<ISchematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService.StoreSchematicAsync(new StoreSchematicRequest
            {
                SchematicBytes = MessagePackSerializer.Serialize(schematic, MessagePack.Resolvers.ContractlessStandardResolver.Instance)
            });

            return MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(response.SchematicBytes, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        public Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            ISchematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default(CancellationToken)) => 
            StoreSchematicAsync(schematic.Copy(), cancellationToken);
    }
}