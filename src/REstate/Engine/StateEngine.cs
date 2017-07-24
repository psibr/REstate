using REstate.Engine.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine.Services;
using REstate.Schematics;

namespace REstate.Engine
{
    public class StateEngine<TState, TInput>
        : ILocalStateEngine<TState, TInput>
    {
        private readonly IStateMachineFactory<TState, TInput> _stateMachineFactory;
        private readonly IRepositoryContextFactory<TState, TInput> _repositoryContextFactory;
        private readonly IConnectorResolver<TState, TInput> _connectorResolver;
        private readonly IReadOnlyCollection<IEventListener> _listeners;

        public StateEngine(
            IStateMachineFactory<TState, TInput> stateMachineFactory,
            IRepositoryContextFactory<TState, TInput> repositoryContextFactory,
            IConnectorResolver<TState, TInput> connectorResolver,
            IEnumerable<IEventListener> listeners)
        {
            _stateMachineFactory = stateMachineFactory;
            _repositoryContextFactory = repositoryContextFactory;
            _connectorResolver = connectorResolver;
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
            MachineStatus<TState, TInput> newMachineStatus;

            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                newMachineStatus = await repositories.Machines
                    .CreateMachineAsync(
                        schematic,
                        metadata,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromSchematic(
                    newMachineStatus.MachineId,
                    schematic);

            NotifyOnMachineCreated(schematic, newMachineStatus, machine);

            await CallOnInitialEntryAction(schematic, newMachineStatus, machine, cancellationToken);

            return machine;
        }

        private async Task CallOnInitialEntryAction(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            IStateMachine<TState, TInput> machine,
            CancellationToken cancellationToken)
        {
            var initialAction = schematic.States[schematic.InitialState].OnEntry;

            if (initialAction == null) return;

            var connector = _connectorResolver.ResolveConnector(initialAction.ConnectorKey);

            await connector.OnInitialEntryAsync(
                schematic,
                machine,
                status,
                initialAction.Settings,
                cancellationToken);
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
            Schematic<TState, TInput> schematic;

            MachineStatus<TState, TInput> newMachineStatus;

            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken);

                newMachineStatus = await repositories.Machines
                    .CreateMachineAsync(
                        schematicName,
                        metadata,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var machine = _stateMachineFactory
                .ConstructFromSchematic(
                    newMachineStatus.MachineId,
                    schematic);

            NotifyOnMachineCreated(schematic, newMachineStatus, machine);

            await CallOnInitialEntryAction(schematic, newMachineStatus, machine, cancellationToken);

            return machine;
        }

        private void NotifyOnMachineCreated(
            ISchematic<TState, TInput> schematic,
            MachineStatus<TState, TInput> machineStatus,
            IStateMachine<TState, TInput> machine)
        {
#pragma warning disable 4014
            Task.Run(async () => await Task.WhenAll(
                _listeners.Select(listener =>
                    listener.OnMachineCreated(
                        new[] { machine },
                        schematic,
                        machineStatus,
                        CancellationToken.None))),
                CancellationToken.None);
#pragma warning restore 4014
        }

        public async Task CreateMachinesBulkAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Schematic<TState, TInput> schematic;

            using (var repositories = _repositoryContextFactory.OpenContext())
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken);

                // TODO: bulk create records (repo call)
            }

            await BulkCallOnInitialEntryAction(schematic, cancellationToken);

        }

        private async Task BulkCallOnInitialEntryAction(
            ISchematic<TState, TInput> schematic,
            CancellationToken cancellationToken)
        {
            var initialAction = schematic.States[schematic.InitialState].OnEntry;

            if (initialAction == null) return;

            var connector = _connectorResolver.ResolveConnector(initialAction.ConnectorKey);

            IBulkEntryConnector<TState, TInput> bulkConnector = null;

            Func<
                ISchematic<TState, TInput>,
                IStateMachine<TState, TInput>,
                Status<TState>,
                IReadOnlyDictionary<string, string>,
                CancellationToken,
            Task> onInitialEntry;

            try
            {
                bulkConnector = connector.GetBulkEntryConnector();

                onInitialEntry = bulkConnector.OnInitialEntryAsync;
            }
            catch (NotSupportedException)
            {
                // Bulk not supported, falling back to individual.
                onInitialEntry = connector.OnInitialEntryAsync;
            }

            await onInitialEntry(schematic, null, default(Status<TState>), initialAction.Settings, cancellationToken);

            if (bulkConnector != null)
                await bulkConnector.ExecuteBulkEntryAsync(cancellationToken);
        }

        public async Task<IStateMachine<TState, TInput>> GetMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Schematic<TState, TInput> schematic;

            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var machineStatus = await repositories.Machines
                    .GetMachineStatusAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);

                schematic = machineStatus.Schematic;
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

            NotifyOnMachineDeleted(machineId);
        }

        private void NotifyOnMachineDeleted(string machineId)
        {
#pragma warning disable 4014
            Task.Run(async () => await Task.WhenAll(
                _listeners.Select(listener =>
                    listener.OnMachineDeleted(
                        new[] { machineId },
                        CancellationToken.None))),
                CancellationToken.None);
#pragma warning restore 4014
        }
    }
}
