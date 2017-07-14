using System;
using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Engine.Services;

namespace REstate.Engine
{
    public class REstateMachineFactory<TState, TInput>
        : IStateMachineFactory<TState, TInput>
    {
        private readonly IConnectorResolver<TState, TInput> _connectorResolver;
        private readonly IRepositoryContextFactory<TState, TInput> _repositoryContextFactory;
        private readonly ICartographer<TState, TInput> _cartographer;

        public REstateMachineFactory(
            IConnectorResolver<TState, TInput> connectorResolver,
            IRepositoryContextFactory<TState, TInput> repositoryContextFactory,
            ICartographer<TState, TInput> cartographer)
        {
            _connectorResolver = connectorResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
        }

        public IStateMachine<TState, TInput> ConstructFromSchematic(string machineId, Schematic<TState, TInput> configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var stateMappings = configuration.StateConfigurations
                .ToDictionary(stateConfig => new State<TState>(stateConfig.Value), stateConfig => stateConfig);

            var reStateMachine = new REstateMachine<TState, TInput>(_connectorResolver, _repositoryContextFactory, _cartographer, machineId, stateMappings);

            return reStateMachine;
        }
    }
}