using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Engine.Services;

namespace REstate.Engine
{
    public class REstateMachine<TState>
        : IStateMachine<TState>
    {
        private readonly IConnectorResolver<TState> _connectorResolver;
        private readonly IRepositoryContextFactory<TState> _repositoryContextFactory;
        private readonly ICartographer<TState> _cartographer;

        protected IDictionary<State<TState>, StateConfiguration<TState>> StateMappings { get; }

        internal REstateMachine(
            IConnectorResolver<TState> connectorResolver,
            IRepositoryContextFactory<TState> repositoryContextFactory,
            ICartographer<TState> cartographer,
            string machineId,
            IDictionary<State<TState>, StateConfiguration<TState>> stateMappings)
        {
            _connectorResolver = connectorResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
            
            MachineId = machineId;

            StateMappings = stateMappings;
        }
        public string MachineId { get; }

        public Task<State<TState>> SendAsync(
            Input input, string payload, 
            CancellationToken cancellationToken)
        {
            return SendAsync(input, payload, null, cancellationToken);
        }

        public async Task<State<TState>> SendAsync(
            Input input, string payload, Guid? lastCommitTag,
            CancellationToken cancellationToken)
        {
            using (var dataContext = _repositoryContextFactory.OpenContext())
            {
                var currentState = await dataContext.Machines
                    .GetMachineStateAsync(MachineId, cancellationToken).ConfigureAwait(false);

                var stateConfig = StateMappings[currentState];

                var transition = stateConfig.Transitions.SingleOrDefault(t => t.InputName == input.InputName);

                if(transition == null)
                {
                    throw new InvalidOperationException($"No transition defined for state: '{currentState.Value}' using input: '{input}'");
                }

                if (transition.Guard != null)
                {
                    var guardConnector =  _connectorResolver.ResolveConnector(transition.Guard.ConnectorKey);

                    if (!await guardConnector
                        .ConstructPredicate(this, transition.Guard.Configuration)
                        .Invoke(currentState, input, payload, cancellationToken).ConfigureAwait(false))
                    {
                        throw new InvalidOperationException("Guard clause prevented transition.");
                    }
                }

                currentState = await dataContext.Machines.SetMachineStateAsync(
                    MachineId,
                    transition.ResultantState, 
                    input, 
                    lastCommitTag ?? currentState.CommitTag, 
                    cancellationToken).ConfigureAwait(false);

                stateConfig = StateMappings[currentState];

                if (stateConfig.OnEntry == null) return currentState;

                try
                {
                    var entryConnector = _connectorResolver.ResolveConnector(stateConfig.OnEntry.ConnectorKey);
                        
                    await entryConnector
                        .ConstructAction(this, currentState, payload, stateConfig.OnEntry.Configuration) 
                        .Invoke(cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    if (stateConfig.OnEntry.FailureTransition == null)
                        throw;

                    currentState = await SendAsync(
                        new Input(stateConfig.OnEntry.FailureTransition.Input),
                        payload,
                        currentState.CommitTag,
                        cancellationToken).ConfigureAwait(false);
                }

                return currentState;
            }
        }

        public async Task<bool> IsInStateAsync(State<TState> state, CancellationToken cancellationToken)
        {
            var currentState = await GetCurrentStateAsync(cancellationToken);

            //If exact match, true
            if(state == currentState)
                return true;

            var configuration = StateMappings[currentState];

            //Recursively look for anscestors
            while(configuration.ParentState != null)
            {
                //If state matches an ancestor, true
                if (configuration.ParentState.Equals(state.Value))
                    return true;
                
                //No match, move one level up the tree
                configuration = StateMappings[new State<TState>(configuration.ParentState)];
            }

            return false;
        }

        public async Task<State<TState>> GetCurrentStateAsync(CancellationToken cancellationToken)
        {
            State<TState> currentState;

            using (var dataContext = _repositoryContextFactory.OpenContext())
            {
                currentState = await dataContext.Machines
                    .GetMachineStateAsync(MachineId, cancellationToken)
                    .ConfigureAwait(false);
            }

            return currentState;
        }

        public async Task<ICollection<Input>> GetPermittedInputAsync(CancellationToken cancellationToken)
        {
            var currentState = await GetCurrentStateAsync(cancellationToken).ConfigureAwait(false);

            var configuration = StateMappings[currentState];

            return configuration.Transitions.Select(t => new Input(t.InputName)).ToList();
        }

        public override string ToString()
        {
            return _cartographer.WriteMap(StateMappings);
        }
    }
}
