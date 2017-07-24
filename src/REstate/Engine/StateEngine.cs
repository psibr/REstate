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

            await connector.OnEntryAsync<object>(
                schematic,
                machine,
                status,
                null,
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
                        schematic,
                        new [] { (Status<TState>)machineStatus },
                        CancellationToken.None))),
                CancellationToken.None);
#pragma warning restore 4014
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
            ICollection<MachineStatus<TState, TInput>> machineStatuses;

            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken))
            {
                machineStatuses = await repositories.Machines.BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
            }

            List<(IStateMachine<TState, TInput> Machine, MachineStatus<TState, TInput> MachineStatus)> machines =
                machineStatuses.Select(status => (_stateMachineFactory
                    .ConstructFromSchematic(
                        status.MachineId,
                        schematic), status)).ToList();

            NotifyBulkOnMachineCreated(schematic, machineStatuses);

            await BulkCallOnInitialEntryAction(schematic, machines, cancellationToken);
        }

        private void NotifyBulkOnMachineCreated(ISchematic<TState, TInput> schematic, IEnumerable<MachineStatus<TState, TInput>> machineStatuses)
        {
#pragma warning disable 4014
            Task.Run(async () => await Task.WhenAll(
                    _listeners.Select(listener =>
                        listener.OnMachineCreated(
                            schematic,
                            machineStatuses.Select(s => (Status<TState>)s).ToList(),
                            CancellationToken.None))),
                CancellationToken.None);
#pragma warning restore 4014
        }

        public async Task BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Schematic<TState, TInput> schematic;

            ICollection<MachineStatus<TState, TInput>> machineStatuses;

            using (var repositories = await _repositoryContextFactory.OpenContextAsync(cancellationToken))
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken);

                machineStatuses = await repositories.Machines.BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
            }

            List<(IStateMachine<TState, TInput> Machine, MachineStatus<TState, TInput> MachineStatus)> machines =
                machineStatuses.Select(status => (_stateMachineFactory
                    .ConstructFromSchematic(
                        status.MachineId,
                        schematic), status)).ToList();

            NotifyBulkOnMachineCreated(schematic, machineStatuses);

            await BulkCallOnInitialEntryAction(schematic, machines, cancellationToken);

        }

        private async Task BulkCallOnInitialEntryAction(
            ISchematic<TState, TInput> schematic,
            IEnumerable<(IStateMachine<TState, TInput> Machine, MachineStatus<TState, TInput> MachineStatus)> machines,
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
                InputParameters<TInput, object>,
                IReadOnlyDictionary<string, string>,
                CancellationToken,
            Task> onInitialEntry;

            try
            {
                bulkConnector = connector.GetBulkEntryConnector();

                onInitialEntry = bulkConnector.OnEntryAsync;
            }
            catch (NotSupportedException)
            {
                // Bulk not supported, falling back to individual.
                onInitialEntry = connector.OnEntryAsync;
            }

            foreach (var machine in machines)
            {
                await onInitialEntry(
                    schematic,
                    machine.Machine,
                    machine.MachineStatus,
                    null,
                    initialAction.Settings,
                    cancellationToken);
            }

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
