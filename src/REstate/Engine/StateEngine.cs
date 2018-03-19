using REstate.Engine.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine.Connectors;
using REstate.Engine.Connectors.Resolution;
using REstate.Engine.EventListeners;
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

        private delegate Task ActionDelegate(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, object> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken);

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
            CancellationToken cancellationToken = default)
        {
            Schematic<TState, TInput> schematic;
            using (var repositories = await _repositoryContextFactory
                .OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken)
                    .ConfigureAwait(false);
            }

            return schematic;
        }

        public async Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default)
        {
            Schematic<TState, TInput> newSchematic;
            using (var repositories = await _repositoryContextFactory
                .OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                newSchematic = await repositories.Schematics
                    .StoreSchematicAsync(schematic, cancellationToken)
                    .ConfigureAwait(false);
            }

            return newSchematic;
        }

        public Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            ISchematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default)
            => StoreSchematicAsync(schematic.Clone(), cancellationToken);

        public Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default) 
            => CreateMachineAsync(schematic, null, metadata, cancellationToken);

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            MachineStatus<TState, TInput> newMachineStatus;

            using (var repositories = await _repositoryContextFactory
                .OpenContextAsync(cancellationToken).ConfigureAwait(false))
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
                    newMachineStatus.MachineId,
                    schematic,
                    new ReadOnlyDictionary<string, string>(
                        newMachineStatus.Metadata ?? new Dictionary<string, string>(0)));

            NotifyOnMachineCreated(schematic, newMachineStatus);

            await InvokeAction(schematic, newMachineStatus, machine, cancellationToken);

            return machine;
        }

        private async Task InvokeAction(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            IStateMachine<TState, TInput> machine,
            CancellationToken cancellationToken)
        {
            var initialAction = schematic.States[schematic.InitialState].Action;

            if (initialAction == null) return;

            var connector = _connectorResolver.ResolveAction(initialAction.ConnectorKey);

            await connector.InvokeAsync<object>(
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
            CancellationToken cancellationToken = default)
            => CreateMachineAsync(schematic, null, metadata, cancellationToken);

        public Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            ISchematic<TState, TInput> schematic,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            => CreateMachineAsync(schematic.Clone(), metadata, cancellationToken);

        public Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            string schematicName,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            => CreateMachineAsync(schematicName, null, metadata, cancellationToken);

        public async Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            string schematicName,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            Schematic<TState, TInput> schematic;

            MachineStatus<TState, TInput> newMachineStatus;

            using (var repositories = await _repositoryContextFactory
                .OpenContextAsync(cancellationToken)
                .ConfigureAwait(false))
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
                    newMachineStatus.MachineId,
                    schematic,
                    new ReadOnlyDictionary<string, string>(
                        newMachineStatus.Metadata ?? new Dictionary<string, string>(0)));

            NotifyOnMachineCreated(schematic, newMachineStatus);

            await InvokeAction(schematic, newMachineStatus, machine, cancellationToken);

            return machine;
        }

        private void NotifyOnMachineCreated(
            ISchematic<TState, TInput> schematic,
            MachineStatus<TState, TInput> machineStatus)
        {
#pragma warning disable 4014
            Task.Run(async () => await Task.WhenAll(
                _listeners.Select(listener =>
                    listener.MachineCreatedAsync(
                        schematic,
                        new []
                        {
                            new MachineCreationEventData<TState>(
                                initialStatus: machineStatus,
                                metadata: new ReadOnlyDictionary<string, string>(
                                    machineStatus.Metadata ?? new Dictionary<string, string>(0))
                            ), 
                        }))),
                CancellationToken.None);
#pragma warning restore 4014
        }

        public Task<IEnumerable<IStateMachine<TState, TInput>>> BulkCreateMachinesAsync(
            ISchematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            return BulkCreateMachinesAsync(schematic.Clone(), metadata, cancellationToken);
        }

        public async Task<IEnumerable<IStateMachine<TState, TInput>>> BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            ICollection<MachineStatus<TState, TInput>> machineStatuses;

            using (var repositories = await _repositoryContextFactory
                .OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                machineStatuses = await repositories.Machines
                    .BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
            }

            var machines = 
                machineStatuses
                    .Select(status => (
                        machine: _stateMachineFactory
                            .ConstructFromSchematic(
                                status.MachineId,
                                schematic,
                                new ReadOnlyDictionary<string, string>(
                                    status.Metadata ?? new Dictionary<string, string>(0))),
                        status))
                    .ToList();

            NotifyBulkOnMachineCreated(schematic, machineStatuses);

            await CallBulkAction(schematic, machines, cancellationToken);

            return machines.Select(tuple => tuple.machine);
        }

        private void NotifyBulkOnMachineCreated(
            ISchematic<TState, TInput> schematic,
            IEnumerable<MachineStatus<TState, TInput>> machineStatuses)
        {
#pragma warning disable 4014
            Task.Run(async () => await Task.WhenAll(
                    _listeners.Select(listener =>
                        listener.MachineCreatedAsync(
                            schematic,
                            machineStatuses.Select(status =>
                                new MachineCreationEventData<TState>(
                                    initialStatus: status,
                                    metadata: new ReadOnlyDictionary<string, string>(
                                        status.Metadata ?? new Dictionary<string, string>(0))
                            )).ToList()))),
                CancellationToken.None);
#pragma warning restore 4014
        }

        public async Task<IEnumerable<IStateMachine<TState, TInput>>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            Schematic<TState, TInput> schematic;

            ICollection<MachineStatus<TState, TInput>> machineStatuses;

            using (var repositories = await _repositoryContextFactory
                .OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                schematic = await repositories.Schematics
                    .RetrieveSchematicAsync(schematicName, cancellationToken);

                machineStatuses = await repositories.Machines
                    .BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
            }

            var machines = machineStatuses
                .Select(status => (
                    machine: _stateMachineFactory
                        .ConstructFromSchematic(
                            machineId: status.MachineId,
                            schematic: schematic,
                            metadata: new ReadOnlyDictionary<string, string>(
                                status.Metadata ?? new Dictionary<string, string>(0))),
                    status))
                .ToList();

            NotifyBulkOnMachineCreated(schematic, machineStatuses);

            await CallBulkAction(schematic, machines, cancellationToken);

            return machines.Select(tuple => tuple.machine);
        }

        private async Task CallBulkAction(
            ISchematic<TState, TInput> schematic,
            IEnumerable<(IStateMachine<TState, TInput> Machine, MachineStatus<TState, TInput> MachineStatus)> machines,
            CancellationToken cancellationToken)
        {
            var initialAction = schematic.States[schematic.InitialState].Action;

            if (initialAction == null) return;

            IBulkActionBatch<TState, TInput> bulkActionBatch = null;
            ActionDelegate stageOrExecute;

            try
            {
                bulkActionBatch = _connectorResolver
                    .ResolveBulkAction(initialAction.ConnectorKey)
                    .CreateBatch();

                stageOrExecute = bulkActionBatch.Stage;
            }
            catch (ConnectorResolutionException)
            {
                // Bulk not supported, falling back to individual.
                var action = _connectorResolver.ResolveAction(initialAction.ConnectorKey);

                stageOrExecute = action.InvokeAsync;
            }

            foreach (var machine in machines)
            {
                await stageOrExecute(
                    schematic,
                    machine.Machine,
                    machine.MachineStatus,
                    null,
                    initialAction.Settings,
                    cancellationToken);
            }

            if (bulkActionBatch != null)
                await bulkActionBatch.ExecuteAsync(cancellationToken);
        }

        public async Task<IStateMachine<TState, TInput>> GetMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            Schematic<TState, TInput> schematic;
            IReadOnlyDictionary<string, string> metadata;

            using (var repositories = await _repositoryContextFactory
                .OpenContextAsync(cancellationToken)
                .ConfigureAwait(false))
            {
                var machineStatus = await repositories.Machines
                    .GetMachineStatusAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);

                schematic = machineStatus.Schematic;
                metadata = new ReadOnlyDictionary<string, string>(
                    machineStatus.Metadata ?? new Dictionary<string, string>(0));
            }

            var machine = _stateMachineFactory
                .ConstructFromSchematic(
                    machineId,
                    schematic,
                    metadata);

            return machine;
        }

        public async Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            var deletionTime = DateTimeOffset.UtcNow;

            using (var repositories = await _repositoryContextFactory
                .OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                await repositories.Machines
                    .DeleteMachineAsync(machineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            NotifyOnMachineDeleted(deletionTime, machineId);
        }

        private void NotifyOnMachineDeleted(DateTimeOffset deletionTime, string machineId)
        {
#pragma warning disable 4014
            Task.Run(async () => await Task.WhenAll(
                _listeners.Select(listener =>
                    listener.MachineDeletedAsync(
                        deletionTime: deletionTime,
                        machineIds: new[] { machineId }))),
                CancellationToken.None);
#pragma warning restore 4014
        }
    }
}
