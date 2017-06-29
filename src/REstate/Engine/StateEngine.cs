using REstate.Configuration;
using REstate.Engine.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine
{
    public class StateEngine : IStateEngine
    {
        private readonly IStateMachineFactory _stateMachineFactory;
        private readonly IRepositoryContextFactory _repositoryContextFactory;

        private readonly StringSerializer _stringSerializer;

        public StateEngine(
            IStateMachineFactory stateMachineFactory,
            IRepositoryContextFactory repositoryContextFactory,
            StringSerializer stringSerializer)
        {
            _stateMachineFactory = stateMachineFactory;
            _repositoryContextFactory = repositoryContextFactory;

            _stringSerializer = stringSerializer;
        }

        public async Task<Schematic> GetSchematicAsync(string schematicName, CancellationToken cancellationToken)
        {
            Schematic configuration;
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
            Schematic schematic;

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

        public string PreviewDiagram(Schematic schematic)
        {
            var machine = _stateMachineFactory
                .ConstructFromConfiguration(null, schematic);

            return machine.ToString();
        }

        public async Task<Schematic> StoreSchematicAsync(Schematic schematic, CancellationToken cancellationToken)
        {
            Schematic newSchematic;
            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                newSchematic = await repositories.Schematics
                    .StoreSchematicAsync(schematic, cancellationToken)
                    .ConfigureAwait(false);
            }

            return newSchematic;
        }

        public async Task<IEnumerable<Schematic>> ListSchematicsAsync(CancellationToken cancellationToken)
        {
            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                return await repositories.Schematics
                    .ListSchematicsAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task<IStateMachine> CreateMachineAsync(Schematic schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken)
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

        public async Task<IStateMachine> GetMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            Schematic schematic;

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

        public async Task<MachineStatus> GetMachineInfoAsync(string machineId, CancellationToken cancellationToken)
        {
            MachineStatus machineStatusInfo;

            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                machineStatusInfo = await repositories.Machines
                    .GetMachineStateAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return machineStatusInfo;
        }
    }
}
