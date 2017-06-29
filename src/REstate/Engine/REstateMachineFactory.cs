using System;
using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;
using REstate.Engine.Repositories;
using REstate.Engine.Services;

namespace REstate.Engine
{
    public class REstateMachineFactory : IStateMachineFactory
    {
        private readonly IConnectorFactoryResolver _connectorFactoryResolver;
        private readonly IRepositoryContextFactory _repositoryContextFactory;
        private readonly ICartographer _cartographer;

        public REstateMachineFactory(
            IConnectorFactoryResolver connectorFactoryResolver,
            IRepositoryContextFactory repositoryContextFactory,
            ICartographer cartographer)
        {
            _connectorFactoryResolver = connectorFactoryResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
        }

        public IStateMachine ConstructFromConfiguration(string machineId, Schematic configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var stateMappings = configuration.StateConfigurations
                .ToDictionary(stateConfig => new State(stateConfig.StateName), stateConfig => stateConfig);

            var reStateMachine = new REstateMachine(_connectorFactoryResolver, _repositoryContextFactory, _cartographer, machineId, stateMappings);

            return reStateMachine;
        }
    }
}