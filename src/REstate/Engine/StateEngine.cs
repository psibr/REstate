using REstate.Engine.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine
{
    public class StateEngine<TState, TInput> 
        : ILocalStateEngine<TState, TInput>
    {
        private readonly IStateMachineFactory<TState, TInput> _stateMachineFactory;
        private readonly IRepositoryContextFactory<TState, TInput> _repositoryContextFactory;
        private readonly IReadOnlyCollection<IEventListener> _listeners;

        public StateEngine(
            IStateMachineFactory<TState, TInput> stateMachineFactory,
            IRepositoryContextFactory<TState, TInput> repositoryContextFactory,
            IEnumerable<IEventListener> listeners)
        {
            _stateMachineFactory = stateMachineFactory;
            _repositoryContextFactory = repositoryContextFactory;
            _listeners = listeners.ToList();
        }

        public async Task<ISchematic<TState, TInput>> GetSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Schematic<TState, TInput> schematic;
            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken)
                    .ConfigureAwait(false);
            }

            return schematic;
        }

        public async Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Schematic<TState, TInput> newSchematic;
            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                newSchematic = await repositories.Schematics
                    .StoreSchematicAsync(schematic, cancellationToken)
                    .ConfigureAwait(false);
            }

            return newSchematic;
        }

        public Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            ISchematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default(CancellationToken))
            => StoreSchematicAsync(schematic.Copy(), cancellationToken);

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var machineId = Guid.NewGuid().ToString();
            MachineStatus<TState, TInput> newMachineStatus;

            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                newMachineStatus = await repositories.Machines
                    .CreateMachineAsync(
                        schematic,
                        machineId,
                        metadata,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromSchematic(
                    machineId,
                    schematic);

            Task.Run(async () => await Task.WhenAll(_listeners.Select(listener => listener.OnMachineCreated(machine, schematic, newMachineStatus, CancellationToken.None))), CancellationToken.None);

            return machine;
        }

        public Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            ISchematic<TState, TInput> schematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
            => CreateMachineAsync(schematic.Copy(), metadata, cancellationToken);

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            string schematicName,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var machineId = Guid.NewGuid().ToString();

            Schematic<TState, TInput> schematic;

            MachineStatus<TState, TInput> newMachineStatus;

            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken);

                newMachineStatus = await repositories.Machines
                    .CreateMachineAsync(
                        schematicName,
                        machineId,
                        metadata,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromSchematic(
                    machineId,
                    schematic);

            Task.Run(async () => await Task.WhenAll(_listeners.Select(listener => listener.OnMachineCreated(machine, schematic, newMachineStatus, CancellationToken.None))), CancellationToken.None);

            return machine;
        }

        public async Task<IStateMachine<TState, TInput>> GetMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Schematic<TState, TInput> schematic;

            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var machineRecord = await repositories.Machines
                    .GetMachineRecordAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);

                schematic = machineRecord.Schematic;
            }

            var machine = _stateMachineFactory
                .ConstructFromSchematic(
                    machineId,
                    schematic);

            return machine;
        }

        public async Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                await repositories.Machines
                    .DeleteMachineAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            Task.Run(async () => await Task.WhenAll(_listeners.Select(listener => listener.OnMachineDeleted(machineId, CancellationToken.None))), CancellationToken.None);
        }
    }
}
