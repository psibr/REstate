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
    public class REstateMachine
        : IStateMachine
    {
        private readonly IConnectorFactoryResolver _connectorFactoryResolver;
        private readonly IRepositoryContextFactory _repositoryContextFactory;
        private readonly ICartographer _cartographer;

        protected IDictionary<State, StateConfiguration> StateMappings { get; }

        internal REstateMachine(
            IConnectorFactoryResolver connectorFactoryResolver,
            IRepositoryContextFactory repositoryContextFactory,
            ICartographer cartographer,
            string machineId,
            IDictionary<State, StateConfiguration> stateMappings)
        {
            _connectorFactoryResolver = connectorFactoryResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
            
            MachineId = machineId;

            StateMappings = stateMappings;
        }
        public string MachineId { get; }

        public Task<MachineStatus> SendAsync(
            Input input, string payload, 
            CancellationToken cancellationToken)
        {
            return SendAsync(input, payload, null, cancellationToken);
        }

        public async Task<MachineStatus> SendAsync(
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
                    throw new InvalidOperationException($"No transition defined for state: '{currentState.StateName}' using input: '{input}'");
                }

                if (transition.Guard != null)
                {
                    var guardConnector = await _connectorFactoryResolver
                        .ResolveConnectorFactory(transition.Guard.ConnectorKey)
                        .BuildConnectorAsync(cancellationToken).ConfigureAwait(false);

                    if (!await guardConnector
                        .ConstructPredicate(this, transition.Guard.Configuration)
                        .Invoke(currentState, input, payload, cancellationToken).ConfigureAwait(false))
                    {
                        throw new InvalidOperationException("Guard clause prevented transition.");
                    }
                }

                currentState = await dataContext.Machines.SetMachineStateAsync(
                    MachineId,
                    transition.ResultantStateName, 
                    input, 
                    lastCommitTag ?? Guid.Parse(currentState.CommitTag), 
                    cancellationToken).ConfigureAwait(false);

                stateConfig = StateMappings[currentState];

                if (stateConfig.OnEntry == null) return currentState;

                try
                {
                    var entryConnector = await _connectorFactoryResolver
                        .ResolveConnectorFactory(stateConfig.OnEntry.ConnectorKey)
                        .BuildConnectorAsync(cancellationToken).ConfigureAwait(false);
                        
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
                        Guid.Parse(currentState.CommitTag),
                        cancellationToken).ConfigureAwait(false);
                }

                return currentState;
            }
        }

        public async Task<bool> IsInStateAsync(State state, CancellationToken cancellationToken)
        {
            var currentState = await GetCurrentStateAsync(cancellationToken);

            //If exact match, true
            if(state == currentState)
                return true;

            var configuration = StateMappings[currentState];

            //Recursively look for anscestors
            while(configuration.ParentStateName != null)
            {
                //If state matches an ancestor, true
                if(configuration.ParentStateName == state.StateName) 
                    return true;
                
                //No match, move one level up the tree
                configuration = StateMappings[new State(configuration.ParentStateName)];
            }

            return false;
        }

        public async Task<MachineStatus> GetCurrentStateAsync(CancellationToken cancellationToken)
        {
            MachineStatus currentState;

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
