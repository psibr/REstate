using System;
using System.Collections.Generic;
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
            => SendAsync(input, payload, default, cancellationToken);

        public Task<Status<TState>> SendAsync(
            TInput input,
            CancellationToken cancellationToken = default) 
            => SendAsync<object>(input, null, default, cancellationToken);

        public Task<Status<TState>> SendAsync(
            TInput input,
            Guid lastCommitTag,
            CancellationToken cancellationToken = default) 
            => SendAsync<object>(input, null, lastCommitTag, cancellationToken);

        public async Task<Status<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            Guid lastCommitTag,
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
                        throw new InvalidOperationException(
                            $"No transition defined for status: '{currentStatus.State}' using input: '{input}'");
                    }

                    if (transition.Guard != null)
                    {
                        var guardConnector = _connectorResolver.ResolveGuardianConnector(transition.Guard.ConnectorKey);

                        if (!await guardConnector.GuardAsync(
                            schematic: Schematic,
                            machine: this,
                            status: currentStatus,
                            inputParameters: new InputParameters<TInput, TPayload>(input, payload),
                            connectorSettings: transition.Guard.Settings,
                            cancellationToken: cancellationToken).ConfigureAwait(false))
                        {
                            throw new InvalidOperationException("Guard clause prevented transition.");
                        }
                    }
                    
                    try
                    {
                        currentStatus = await dataContext.Machines.SetMachineStateAsync(
                            machineId: MachineId,
                            state: transition.ResultantState,
                            lastCommitTag: lastCommitTag == default ? currentStatus.CommitTag : lastCommitTag,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    catch (StateConflictException)
                    {
                        if (Schematic.StateConflictRetryCount == -1 
                            || retries < Schematic.StateConflictRetryCount)
                        {
                            retries++;
                            continue;
                        }

                        throw;
                    }

                    schematicState = Schematic.States[currentStatus.State];

                    var preActionState = currentStatus;

                    NotifyOnTransition(input, payload, preActionState);

                    if (schematicState.OnEntry == null) return currentStatus;

                    try
                    {
                        var entryConnector =
                            _connectorResolver.ResolveEntryConnector(schematicState.OnEntry.ConnectorKey);

                        await entryConnector.OnEntryAsync(
                            schematic: Schematic,
                            machine: this,
                            status: currentStatus,
                            inputParameters: new InputParameters<TInput, TPayload>(input, payload),
                            connectorSettings: schematicState.OnEntry.Settings,
                            cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        if (schematicState.OnEntry.OnExceptionInput == null)
                            throw;

                        currentStatus = await SendAsync(
                            input: schematicState.OnEntry.OnExceptionInput.Input,
                            payload: payload,
                            lastCommitTag: currentStatus.CommitTag,
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

            //Recursively look for anscestors
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

        protected async Task<Status<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default)
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
    }
}