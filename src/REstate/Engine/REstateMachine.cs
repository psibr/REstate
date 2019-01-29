using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine.Connectors;
using REstate.Engine.Connectors.Resolution;
using REstate.Engine.EventListeners;
using REstate.Engine.Repositories;
using REstate.Schematics;

namespace REstate.Engine
{
    public class REstateMachine<TState, TInput>
        : IStateMachine<TState, TInput>
    {
        private readonly IConnectorResolver<TState, TInput> _connectorResolver;
        private readonly IMachineStatusStore<TState, TInput> _machineStatusStore;
        private readonly IReadOnlyCollection<IEventListener> _listeners;

        internal REstateMachine(
            IConnectorResolver<TState, TInput> connectorResolver,
            IMachineStatusStore<TState, TInput> machineStatusStore,
            IEnumerable<IEventListener> listeners,
            string machineId,
            Schematic<TState, TInput> schematic,
            ReadOnlyDictionary<string, string> metadata)
        {
            _connectorResolver = connectorResolver;
            _listeners = listeners.ToList();
            _machineStatusStore = machineStatusStore;

            MachineId = machineId;

            CachedValues = new Lazy<Task<(ISchematic<TState, TInput> schematic, IReadOnlyDictionary<string, string> metadata)>>(
                () => Task.FromResult(((ISchematic<TState, TInput>)schematic, (IReadOnlyDictionary<string, string>)metadata)));
        }

        internal REstateMachine(
            IConnectorResolver<TState, TInput> connectorResolver,
            IMachineStatusStore<TState, TInput> machineStatusStore,
            IEnumerable<IEventListener> listeners,
            string machineId)
        {
            _machineStatusStore = machineStatusStore;
            _connectorResolver = connectorResolver;
            _listeners = listeners.ToList();

            MachineId = machineId;

            CachedValues = new Lazy<Task<(ISchematic<TState, TInput> schematic, IReadOnlyDictionary<string, string> metadata)>>(
                async () =>
                {
                    var machineStatus = await _machineStatusStore.GetMachineStatusAsync(CancellationToken.None)
                        .ConfigureAwait(false);

                    return (machineStatus.Schematic,
                        new ReadOnlyDictionary<string, string>(machineStatus.Metadata));
                });
        }

        protected readonly Lazy<Task<(ISchematic<TState, TInput> schematic, IReadOnlyDictionary<string, string> metadata)>> CachedValues;

        public async Task<ISchematic<TState, TInput>> GetSchematicAsync(
            CancellationToken cancellationToken) => (await CachedValues.Value).schematic;

        public string MachineId { get; }

        public async Task<IReadOnlyDictionary<string, string>> GetMetadataAsync(
            CancellationToken cancellationToken = default) => (await CachedValues.Value).metadata;

        public Task<Status<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload,
            CancellationToken cancellationToken = default)
            => SendAsync(input, payload, null, null, cancellationToken);

        public Task<Status<TState>> SendAsync(
            TInput input,
            CancellationToken cancellationToken = default)
            => SendAsync<object>(input, null, null, null, cancellationToken);

        public Task<Status<TState>> SendAsync(
            TInput input,
            long lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default)
            => SendAsync<object>(input, null, (long?)lastCommitNumber, stateBag, cancellationToken);

        public Task<Status<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload,
            long lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default)
            => SendAsync(input, payload, (long?)lastCommitNumber, stateBag, cancellationToken);

        private async Task<Status<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload,
            long? lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default)
        {
            int retries = 0;

            while (true)
            {
                var machineStatus = await _machineStatusStore.GetMachineStatusAsync(cancellationToken)
                    .ConfigureAwait(false);

                Status<TState> currentStatus = machineStatus;

                ISchematic<TState, TInput> schematic = machineStatus.Schematic;

                var schematicState = schematic.States[currentStatus.State];

                if (!schematicState.Transitions.TryGetValue(input, out var transition))
                {
                    throw new TransitionNotDefinedException(currentStatus.State.ToString(), input.ToString());
                }

                if (transition.Precondition != null)
                {
                    var precondition = _connectorResolver
                        .ResolvePrecondition(transition.Precondition.ConnectorKey);

                    if (!await precondition.ValidateAsync(
                        schematic: schematic,
                        machine: this,
                        status: currentStatus,
                        inputParameters: new InputParameters<TInput, TPayload>(input, payload),
                        connectorSettings: transition.Precondition.Settings,
                        cancellationToken: cancellationToken).ConfigureAwait(false))
                    {
                        throw new TransitionFailedPreconditionException(currentStatus.State.ToString(), input.ToString(), transition.ResultantState.ToString());
                    }
                }
                var resultantState = schematic.States[transition.ResultantState];
                if (resultantState.Precondition != null)
                {
                    var precondition = _connectorResolver
                        .ResolvePrecondition(resultantState.Precondition.ConnectorKey);

                    if (!await precondition.ValidateAsync(
                        schematic: schematic,
                        machine: this,
                        status: currentStatus,
                        inputParameters: new InputParameters<TInput, TPayload>(input, payload),
                        connectorSettings: resultantState.Precondition.Settings,
                        cancellationToken: cancellationToken).ConfigureAwait(false))
                    {
                        throw new TransitionFailedPreconditionException(currentStatus.State.ToString(), input.ToString(), transition.ResultantState.ToString());
                    }
                }

                try
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if (_machineStatusStore is IOptimisticallyConcurrentMachineStatusStore<TState, TInput> optimisticMode)
                        machineStatus = await optimisticMode.SetMachineStateAsync(
                            machineStatus: machineStatus,
                            state: transition.ResultantState,
                            lastCommitNumber: lastCommitNumber ?? currentStatus.CommitNumber,
                            stateBag: stateBag,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                    else
                        machineStatus = await _machineStatusStore.SetMachineStateAsync(
                            state: transition.ResultantState,
                            lastCommitNumber: lastCommitNumber ?? currentStatus.CommitNumber,
                            stateBag: stateBag,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                catch (StateConflictException)
                {
                    if ((schematic.StateConflictRetryCount == -1 || retries < schematic.StateConflictRetryCount)
                            && lastCommitNumber != null /*If a specific commit number was requested, retrying is worthless*/)
                    {
                        retries++;
                        continue;
                    }

                    throw;
                }

                currentStatus = machineStatus;

                schematicState = schematic.States[currentStatus.State];

                var preActionState = machineStatus;

                NotifyOnTransition(input, payload, preActionState);

                if (schematicState.Action == null) return currentStatus;

                try
                {
                    var action =
                        _connectorResolver.ResolveAction(schematicState.Action.ConnectorKey);

                    InterceptorMachine interceptor;
                    using (interceptor = new InterceptorMachine(this, currentStatus))
                    {
                        await action.InvokeAsync(
                            schematic: schematic,
                            machine: interceptor,
                            status: currentStatus,
                            inputParameters: new InputParameters<TInput, TPayload>(input, payload),
                            connectorSettings: schematicState.Action.Settings,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                    }

                    currentStatus = interceptor.CurrentStatus;
                }
                catch
                {
                    if (schematicState.Action.OnExceptionInput == null)
                        throw;

                    currentStatus = await SendAsync(
                        input: schematicState.Action.OnExceptionInput.Input,
                        payload: payload,
                        lastCommitNumber: currentStatus.CommitNumber,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }

                return currentStatus;
            }
        }

        private void NotifyOnTransition<TPayload>(TInput input, TPayload payload, MachineStatus<TState, TInput> status)
        {
#pragma warning disable 4014
            Task.Run(async () =>
                {
                    await Task.WhenAll(
                        _listeners.Select(listener =>
                            listener.TransitionAsync(
                                schematic: status.Schematic,
                                status: status,
                                metadata: new ReadOnlyDictionary<string, string>(status.Metadata),
                                input: input,
                                payload: payload)));
                },
                CancellationToken.None);
#pragma warning restore 4014
        }

        public async Task<Status<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default)
        {
            var currentStatus = await _machineStatusStore
                .GetMachineStatusAsync(cancellationToken)
                .ConfigureAwait(false);

            return currentStatus;
        }

        public override string ToString() => MachineId;

        private class InterceptorMachine
            : IStateMachine<TState, TInput>
            , IDisposable
        {
            private bool _isDisposed;
            private Status<TState> _currentStatus;

            public InterceptorMachine(IStateMachine<TState, TInput> machine, Status<TState> currentStatus)
            {
                _isDisposed = false;
                Machine = machine;
                CurrentStatus = currentStatus;
            }

            public string MachineId => Machine.MachineId;

            private IStateMachine<TState, TInput> Machine { get; }

            public Status<TState> CurrentStatus
            {
                get => _currentStatus;
                private set
                {
                    lock (this)
                    {
                        if (value.CommitNumber > _currentStatus.CommitNumber)
                            _currentStatus = value;
                    }
                }
            }

            public Task<IReadOnlyDictionary<string, string>> GetMetadataAsync(
                CancellationToken cancellationToken = default)
                => Machine.GetMetadataAsync(cancellationToken);

            public Task<ISchematic<TState, TInput>> GetSchematicAsync(
                CancellationToken cancellationToken = default)
                => Machine.GetSchematicAsync(cancellationToken);

            public Task<Status<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default)
                => Machine.GetCurrentStateAsync(cancellationToken);


            public async Task<Status<TState>> SendAsync<TPayload>(
                TInput input,
                TPayload payload,
                CancellationToken cancellationToken = default)
            {
                if (_isDisposed) throw new ObjectDisposedException("IStateMachine<,>", "Cannot send input at this point as final state has been captured.");

                var status = await Machine.SendAsync(input, payload, cancellationToken);

                CurrentStatus = status;

                return status;
            }

            public async Task<Status<TState>> SendAsync<TPayload>(
                TInput input,
                TPayload payload,
                long lastCommitNumber,
                IDictionary<string, string> stateBag = null,
                CancellationToken cancellationToken = default)
            {
                if (_isDisposed) throw new ObjectDisposedException("IStateMachine<,>", "Cannot send input at this point as final state has been captured.");

                var status = await Machine.SendAsync(input, payload, lastCommitNumber, stateBag, cancellationToken);

                CurrentStatus = status;

                return status;
            }

            public async Task<Status<TState>> SendAsync(
                TInput input,
                CancellationToken cancellationToken = default)
            {
                if (_isDisposed) throw new ObjectDisposedException("IStateMachine<,>", "Cannot send input at this point as final state has been captured.");

                var status = await Machine.SendAsync(input, cancellationToken);

                CurrentStatus = status;

                return status;
            }

            public async Task<Status<TState>> SendAsync(
                TInput input,
                long lastCommitNumber,
                IDictionary<string, string> stateBag = null,
                CancellationToken cancellationToken = default)
            {
                if (_isDisposed) throw new ObjectDisposedException("IStateMachine<,>", "Cannot send input at this point as final state has been captured.");

                var status = await Machine.SendAsync(input, lastCommitNumber, stateBag, cancellationToken);

                CurrentStatus = status;

                return status;
            }

            /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
            public void Dispose()
            {
                _isDisposed = true;
            }
        }
    }
}
