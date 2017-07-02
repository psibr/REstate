using System;
using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Engine.Services;

namespace REstate.Engine
{
    public class REstateMachineFactory<TState>
        : IStateMachineFactory<TState>
    {
        private readonly IConnectorResolver<TState> _connectorResolver;
        private readonly IRepositoryContextFactory<TState> _repositoryContextFactory;
        private readonly ICartographer<TState> _cartographer;

        public REstateMachineFactory(
            IConnectorResolver<TState> connectorResolver,
            IRepositoryContextFactory<TState> repositoryContextFactory,
            ICartographer<TState> cartographer)
        {
            _connectorResolver = connectorResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
        }

        public IStateMachine<TState> ConstructFromConfiguration(string machineId, Schematic<TState> configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var stateMappings = configuration.StateConfigurations
                .ToDictionary(stateConfig => new State<TState>(stateConfig.Value), stateConfig => stateConfig);

            var reStateMachine = new REstateMachine<TState>(_connectorResolver, _repositoryContextFactory, _cartographer, machineId, stateMappings);

            return reStateMachine;
        }
    }
}