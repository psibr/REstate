using REstate.Configuration;
using REstate.Engine.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine
{
    public class StateEngine<TState, TInput> 
        : IStateEngine<TState, TInput>
    {
        private readonly IStateMachineFactory<TState, TInput> _stateMachineFactory;
        private readonly IRepositoryContextFactory<TState, TInput> _repositoryContextFactory;

        public StateEngine(
            IStateMachineFactory<TState, TInput> stateMachineFactory,
            IRepositoryContextFactory<TState, TInput> repositoryContextFactory)
        {
            _stateMachineFactory = stateMachineFactory;
            _repositoryContextFactory = repositoryContextFactory;
        }

        public async Task<Schematic<TState, TInput>> GetSchematicAsync(string schematicName, CancellationToken cancellationToken)
        {
            Schematic<TState, TInput> configuration;
            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                configuration = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken)
                    .ConfigureAwait(false);
            }

            return configuration;
        }

        public async Task<string> GetSchematicDiagramAsync(string schematicName, CancellationToken cancellationToken)
        {
            Schematic<TState, TInput> schematic;

            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromConfiguration(null, schematic);

            return machine.ToString();
        }

        public string PreviewDiagram(Schematic<TState, TInput> schematic)
        {
            var machine = _stateMachineFactory
                .ConstructFromConfiguration(null, schematic);

            return machine.ToString();
        }

        public async Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken)
        {
            Schematic<TState, TInput> newSchematic;
            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                newSchematic = await repositories.Schematics
                    .StoreSchematicAsync(schematic, cancellationToken)
                    .ConfigureAwait(false);
            }

            return newSchematic;
        }

        public async Task<IEnumerable<Schematic<TState, TInput>>> ListSchematicsAsync(CancellationToken cancellationToken)
        {
            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                return await repositories.Schematics
                    .ListSchematicsAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(Schematic<TState, TInput> schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var machineId = Guid.NewGuid().ToString();

            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                await repositories.Machines
                    .CreateMachineAsync(schematic, machineId, metadata, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromConfiguration(
                    machineId,
                    schematic);

            return machine;
        }

        public async Task<string> CreateMachineAsync(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var machineId = Guid.NewGuid().ToString();

            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                await repositories.Machines
                    .CreateMachineAsync(schematicName, machineId, metadata, cancellationToken)
                    .ConfigureAwait(false);
            }

            return machineId;
        }
        public async Task<IDictionary<string, string>> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken)
        {
            IDictionary<string, string> metadata;

            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                metadata = await repositories.Machines
                    .GetMachineMetadataAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return metadata;
        }

        public async Task<IStateMachine<TState, TInput>> GetMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            Schematic<TState, TInput> schematic;

            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicForMachineAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromConfiguration(
                    machineId,
                    schematic);

            return machine;
        }

        public async Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                await repositories.Machines
                    .DeleteMachineAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<State<TState>> GetMachineInfoAsync(string machineId, CancellationToken cancellationToken)
        {
            State<TState> state;

            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                state = await repositories.Machines
                    .GetMachineStateAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return state;
        }
    }
}
