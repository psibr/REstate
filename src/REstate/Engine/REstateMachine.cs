using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine.Connectors;
using REstate.Engine.Connectors.Resolution;
using REstate.Engine.EventListeners;
using REstate.Engine.Repositories;
using REstate.Logging;
using REstate.Schematics;

namespace REstate.Engine
{
    public class REstateMachine<TState, TInput>
        : IStateMachine<TState, TInput>
    {
        private readonly IConnectorResolver<TState, TInput> _connectorResolver;
        private readonly IRepositoryContextFactory<TState, TInput> _repositoryContextFactory;
        private readonly IReadOnlyDictionary<string, string> _metadata;
        private readonly IReadOnlyCollection<IEventListener> _listeners;

        internal REstateMachine(
            IConnectorResolver<TState, TInput> connectorResolver,
            IRepositoryContextFactory<TState, TInput> repositoryContextFactory,
            ICartographer<TState, TInput> cartographer,
            IEnumerable<IEventListener> listeners,
            string machineId,
            ISchematic<TState, TInput> schematic,
            IReadOnlyDictionary<string, string> metadata)
        {
            _connectorResolver = connectorResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _metadata = metadata;
            _listeners = listeners.ToList();

            MachineId = machineId;

            Schematic = schematic;
        }

        public ISchematic<TState, TInput> Schematic { get; }

        Task<ISchematic<TState, TInput>> IStateMachine<TState, TInput>.GetSchematicAsync(
            CancellationToken cancellationToken) => Task.FromResult(Schematic);

        public string MachineId { get; }

        public Task<IReadOnlyDictionary<string, string>> GetMetadataAsync(
            CancellationToken cancellationToken = default) 
            => Task.FromResult(_metadata);

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
            using (var dataContext = await  _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                int retries = 0;

                while (true)
                {
                    Status<TState> currentStatus = await dataContext.Machines
                        .GetMachineStatusAsync(MachineId, cancellationToken).ConfigureAwait(false);

                    var schematicState = Schematic.States[currentStatus.State];

                    if (!schematicState.Transitions.TryGetValue(input, out var transition))
                    {
                        throw new TransitionNotDefinedException(currentStatus.State.ToString(), input.ToString());
                    }

                    if (transition.Precondition != null)
                    {
                        var precondition = _connectorResolver
                            .ResolvePrecondition(transition.Precondition.ConnectorKey);

                        if (!await precondition.ValidateAsync(
                            schematic: Schematic,
                            machine: this,
                            status: currentStatus,
                            inputParameters: new InputParameters<TInput, TPayload>(input, payload),
                            connectorSettings: transition.Precondition.Settings,
                            cancellationToken: cancellationToken).ConfigureAwait(false))
                        {
                            throw new TransitionFailedPreconditionException(currentStatus.State.ToString(), input.ToString(), transition.ResultantState.ToString());
                        }
                    }
                    var resultantState = Schematic.States[transition.ResultantState];
                    if (resultantState.Precondition != null)
                    {
                        var precondition = _connectorResolver
                            .ResolvePrecondition(resultantState.Precondition.ConnectorKey);

                        if (!await precondition.ValidateAsync(
                            schematic: Schematic,
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
                        currentStatus = await dataContext.Machines.SetMachineStateAsync(
                            machineId: MachineId,
                            state: transition.ResultantState,
                            lastCommitNumber: lastCommitNumber ?? currentStatus.CommitNumber,
                            stateBag: stateBag,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    catch (StateConflictException)
                    {
                        if ((Schematic.StateConflictRetryCount == -1 || retries < Schematic.StateConflictRetryCount)
                                && lastCommitNumber != null /*If a specific commit number was requested, retrying is worthless*/)
                        {
                            retries++;
                            continue;
                        }

                        throw;
                    }

                    schematicState = Schematic.States[currentStatus.State];

                    var preActionState = currentStatus;

                    NotifyOnTransition(input, payload, preActionState);

                    if (schematicState.Action == null) return currentStatus;

                    try
                    {
                        var action =
                            _connectorResolver.ResolveAction(schematicState.Action.ConnectorKey);

                        async Task<Status<TState>> InvokeAndCaptureAsync(Status<TState> originalStatus)
                        {
                            InterceptorMachine interceptor;
                            using (interceptor = new InterceptorMachine(this, originalStatus))
                            {
                                await action.InvokeAsync(
                                    schematic: Schematic,
                                    machine: interceptor,
                                    status: originalStatus,
                                    inputParameters: new InputParameters<TInput, TPayload>(input, payload),
                                    connectorSettings: schematicState.Action.Settings,
                                    cancellationToken: cancellationToken).ConfigureAwait(false);
                            }

                            return interceptor.CurrentStatus;
                        }

                        if (schematicState.Action.LongRunning)
                        {
                            var originalStatus = currentStatus;
#pragma warning disable 4014
                            Task.Run(() => InvokeAndCaptureAsync(originalStatus), CancellationToken.None)
                                .ContinueWith(t =>
                                {
                                    LogProvider.GetLogger(action.GetType())
                                            .ErrorException("Long running action experienced an unhandled exception.",
                                                t.Exception?.InnerException, TaskContinuationOptions.OnlyOnFaulted);
                                }, CancellationToken.None);
#pragma warning restore 4014

                            return currentStatus;
                        }

                        currentStatus = await InvokeAndCaptureAsync(currentStatus);
                        
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
        }

        private void NotifyOnTransition<TPayload>(TInput input, TPayload payload, Status<TState> status)
        {
#pragma warning disable 4014
            Task.Run(async () => await Task.WhenAll(
                    _listeners.Select(listener =>
                        listener.TransitionAsync(
                            schematic: Schematic,
                            status: status,
                            metadata: _metadata,
                            input: input,
                            payload: payload))),
                CancellationToken.None);
#pragma warning restore 4014
        }

        public async Task<bool> IsInStateAsync(Status<TState> status, CancellationToken cancellationToken = default)
        {
            var currentState = await GetCurrentStateAsync(cancellationToken);

            //If exact match, true
            if(status == currentState)
                return true;

            var configuration = Schematic.States[currentState.State];

            //Recursively look for ancestors
            while(configuration.ParentState != null)
            {
                //If status matches an ancestor, true
                if (configuration.ParentState.Equals(status.State))
                    return true;
                
                //No match, move one level up the tree
                configuration = Schematic.States[configuration.ParentState];
            }

            return false;
        }

        public async Task<Status<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default)
        {
            Status<TState> currentStatus;

            using (var dataContext = await _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                currentStatus = await dataContext.Machines
                    .GetMachineStatusAsync(MachineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return currentStatus;
        }

        public override string ToString() =>
            $"{Schematic.SchematicName}/{MachineId}";

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
                        if(value.CommitNumber > _currentStatus.CommitNumber)
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
                if(_isDisposed) throw new ObjectDisposedException("IStateMachine<,>", "Cannot send input at this point as final state has been captured.");

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
                if(_isDisposed) throw new ObjectDisposedException("IStateMachine<,>", "Cannot send input at this point as final state has been captured.");

                var status = await Machine.SendAsync(input, payload, lastCommitNumber, stateBag, cancellationToken);

                CurrentStatus = status;

                return status;
            }

            public async Task<Status<TState>> SendAsync(
                TInput input,
                CancellationToken cancellationToken = default)
            {
                if(_isDisposed) throw new ObjectDisposedException("IStateMachine<,>", "Cannot send input at this point as final state has been captured.");

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
                if(_isDisposed) throw new ObjectDisposedException("IStateMachine<,>", "Cannot send input at this point as final state has been captured.");

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
