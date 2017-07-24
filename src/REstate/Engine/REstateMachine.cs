using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine.Repositories;
using REstate.Engine.Services;
using REstate.Schematics;

namespace REstate.Engine
{
    public class REstateMachine<TState, TInput>
        : IStateMachine<TState, TInput>
    {
        private readonly IConnectorResolver<TState, TInput> _connectorResolver;
        private readonly IRepositoryContextFactory<TState, TInput> _repositoryContextFactory;
        private readonly ICartographer<TState, TInput> _cartographer;
        private readonly IReadOnlyCollection<IEventListener> _listeners;

        internal REstateMachine(
            IConnectorResolver<TState, TInput> connectorResolver,
            IRepositoryContextFactory<TState, TInput> repositoryContextFactory,
            ICartographer<TState, TInput> cartographer,
            IEnumerable<IEventListener> listeners,
            string machineId,
            ISchematic<TState, TInput> schematic)
        {
            _connectorResolver = connectorResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
            _listeners = listeners.ToList();

            MachineId = machineId;

            Schematic = schematic;
        }

        public ISchematic<TState, TInput> Schematic { get; }

        Task<ISchematic<TState, TInput>> IStateMachine<TState, TInput>.GetSchematicAsync(
            CancellationToken cancellationToken) => Task.FromResult(Schematic);

        public string MachineId { get; }

        public Task<Status<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(input, payload, default(Guid), cancellationToken);
        }

        public Task<Status<TState>> SendAsync(
            TInput input,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync<object>(input, null, default(Guid), cancellationToken);
        }

        public Task<Status<TState>> SendAsync(
            TInput input,
            Guid lastCommitTag,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync<object>(input, null, lastCommitTag, cancellationToken);
        }

        public async Task<Status<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            Guid lastCommitTag,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var dataContext = await  _repositoryContextFactory.OpenContextAsync(cancellationToken).ConfigureAwait(false))
            {
                Status<TState> currentStatus = await dataContext.Machines
                    .GetMachineStatusAsync(MachineId, cancellationToken).ConfigureAwait(false);

                var schematicState = Schematic.States[currentStatus.State];

                if(!schematicState.Transitions.TryGetValue(input, out ITransition<TState, TInput> transition))
                {
                    throw new InvalidOperationException($"No transition defined for status: '{currentStatus.State}' using input: '{input}'");
                }

                if (transition.Guard != null)
                {
                    var guardConnector =  _connectorResolver.ResolveConnector(transition.Guard.ConnectorKey);

                    if (!await guardConnector.GuardAsync(Schematic, this, currentStatus, input, payload, transition.Guard.Settings, cancellationToken).ConfigureAwait(false))
                    {
                        throw new InvalidOperationException("Guard clause prevented transition.");
                    }
                }

                currentStatus = await dataContext.Machines.SetMachineStateAsync(
                    MachineId,
                    transition.ResultantState, 
                    input, 
                    lastCommitTag == default(Guid) ? currentStatus.CommitTag : lastCommitTag, 
                    cancellationToken).ConfigureAwait(false);

                schematicState = Schematic.States[currentStatus.State];

                var preActionState = currentStatus;

                NotifyOnTransition(input, payload, preActionState);

                if (schematicState.OnEntry == null) return currentStatus;

                try
                {
                    var entryConnector = _connectorResolver.ResolveConnector(schematicState.OnEntry.ConnectorKey);
                        
                    await entryConnector.OnEntryAsync(Schematic, this, currentStatus, input, payload, schematicState.OnEntry.Settings, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    if (schematicState.OnEntry.OnFailureInput == null)
                        throw;

                    currentStatus = await SendAsync(
                        schematicState.OnEntry.OnFailureInput,
                        payload,
                        currentStatus.CommitTag,
                        cancellationToken).ConfigureAwait(false);
                }

                return currentStatus;
            }
        }

        private void NotifyOnTransition<TPayload>(TInput input, TPayload payload, Status<TState> status)
        {
#pragma warning disable 4014
            Task.Run(async () => await Task.WhenAll(
                    _listeners.Select(listener =>
                        listener.OnTransition(
                            this,
                            Schematic,
                            status,
                            input,
                            payload,
                            CancellationToken.None))),
                CancellationToken.None);
#pragma warning restore 4014
        }

        public async Task<bool> IsInStateAsync(Status<TState> status, CancellationToken cancellationToken = default(CancellationToken))
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

        public async Task<Status<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default(CancellationToken))
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

        public async Task<ICollection<TInput>> GetPermittedInputAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentState = await GetCurrentStateAsync(cancellationToken).ConfigureAwait(false);

            var configuration = Schematic.States[currentState.State];

            return configuration.Transitions.Values.Select(t => t.Input).ToList();
        }

        public override string ToString()
        {
            return _cartographer.WriteMap(Schematic.States.Values);
        }
    }
}