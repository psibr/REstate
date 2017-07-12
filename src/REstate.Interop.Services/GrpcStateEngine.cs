using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MessagePack;
using REstate.Configuration;
using REstate.Interop.Models;
using REstate.Interop.Services;

namespace REstate.Interop
{
    public class GrpcStateEngine<TState, TInput>
        : IStateEngine<TState, TInput>
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

        public Task<IStateMachine<TState, TInput>> CreateMachineAsync(Schematic<TState, TInput> schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<string> CreateMachineAsync(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<IStateMachine<TState, TInput>> GetMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IStateMachine<TState, TInput>>(new GrpcStateMachine<TState, TInput>(_stateMachineService, machineId));
        }

        public Task<State<TState>> GetMachineInfoAsync(string machineId, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<IDictionary<string, string>> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<Schematic<TState, TInput>> GetSchematicAsync(string schematicName, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<string> GetSchematicDiagramAsync(string schematicName, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<IEnumerable<Schematic<TState, TInput>>> ListSchematicsAsync(CancellationToken cancellationToken)
        {
            return null;
        }

        public string PreviewDiagram(Schematic<TState, TInput> schematic)
        {
            return null;
        }

        public async Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken)
        {
            var response = await _stateMachineService.StoreSchematicAsync(new StoreSchematicRequest
            {
                SchematicBytes = MessagePackSerializer.Serialize(schematic, MessagePack.Resolvers.ContractlessStandardResolver.Instance)
            });

            return MessagePackSerializer.Deserialize<Schematic<TState, TInput>>((byte[]) response.SchematicBytes, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }
    }
}