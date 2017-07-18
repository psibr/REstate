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

        internal REstateMachine(
            IConnectorResolver<TState, TInput> connectorResolver,
            IRepositoryContextFactory<TState, TInput> repositoryContextFactory,
            ICartographer<TState, TInput> cartographer,
            string machineId,
            ISchematic<TState, TInput> schematic)
        {
            _connectorResolver = connectorResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
            
            MachineId = machineId;

            Schematic = schematic;
        }

        public ISchematic<TState, TInput> Schematic { get; }

        Task<ISchematic<TState, TInput>> IStateMachine<TState, TInput>.GetSchematicAsync(
            CancellationToken cancellationToken) => Task.FromResult(Schematic);

        public string MachineId { get; }

        public Task<State<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(input, payload, default(Guid), cancellationToken);
        }

        public Task<State<TState>> SendAsync(
            TInput input,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync<object>(input, null, default(Guid), cancellationToken);
        }

        public Task<State<TState>> SendAsync(
            TInput input,
            Guid lastCommitTag,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync<object>(input, null, lastCommitTag, cancellationToken);
        }

        public async Task<State<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            Guid lastCommitTag,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var dataContext = _repositoryContextFactory.OpenContext())
            {
                State<TState> currentState = await dataContext.Machines
                    .GetMachineRecordAsync(MachineId, cancellationToken).ConfigureAwait(false);

                var stateConfig = Schematic.States[currentState.Value];

                if(!stateConfig.Transitions.TryGetValue(input, out ITransition<TState, TInput> transition))
                {
                    throw new InvalidOperationException($"No transition defined for state: '{currentState.Value}' using input: '{input}'");
                }

                if (transition.Guard != null)
                {
                    var guardConnector =  _connectorResolver.ResolveConnector(transition.Guard.ConnectorKey);

                    if (!await guardConnector.GuardAsync(this, currentState, input, payload, transition.Guard.Settings, cancellationToken).ConfigureAwait(false))
                    {
                        throw new InvalidOperationException("Guard clause prevented transition.");
                    }
                }

                currentState = await dataContext.Machines.SetMachineStateAsync(
                    MachineId,
                    transition.ResultantState, 
                    input, 
                    lastCommitTag == default(Guid) ? currentState.CommitTag : lastCommitTag, 
                    cancellationToken).ConfigureAwait(false);

                stateConfig = Schematic.States[currentState.Value];

                if (stateConfig.OnEntry == null) return currentState;

                try
                {
                    var entryConnector = _connectorResolver.ResolveConnector(stateConfig.OnEntry.ConnectorKey);
                        
                    await entryConnector.OnEntryAsync(this, currentState, input, payload, stateConfig.OnEntry.Settings, cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    if (stateConfig.OnEntry.OnFailureInput == null)
                        throw;

                    currentState = await SendAsync(
                        stateConfig.OnEntry.OnFailureInput,
                        payload,
                        currentState.CommitTag,
                        cancellationToken).ConfigureAwait(false);
                }

                return currentState;
            }
        }

        public async Task<bool> IsInStateAsync(State<TState> state, CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentState = await GetCurrentStateAsync(cancellationToken);

            //If exact match, true
            if(state == currentState)
                return true;

            var configuration = Schematic.States[currentState.Value];

            //Recursively look for anscestors
            while(configuration.ParentState != null)
            {
                //If state matches an ancestor, true
                if (configuration.ParentState.Equals(state.Value))
                    return true;
                
                //No match, move one level up the tree
                configuration = Schematic.States[configuration.ParentState];
            }

            return false;
        }

        public async Task<State<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            State<TState> currentState;

            using (var dataContext = _repositoryContextFactory.OpenContext())
            {
                currentState = await dataContext.Machines
                    .GetMachineRecordAsync(MachineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return currentState;
        }

        public async Task<ICollection<TInput>> GetPermittedInputAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var currentState = await GetCurrentStateAsync(cancellationToken).ConfigureAwait(false);

            var configuration = Schematic.States[currentState.Value];

            return configuration.Transitions.Values.Select(t => t.Input).ToList();
        }

        public override string ToString()
        {
            return _cartographer.WriteMap(Schematic.States.Values);
        }
    }
}