using System;
using System.Collections.Generic;
using System.Linq;
using REstate.Engine.Repositories;
using REstate.Engine.Services;
using REstate.Schematics;

namespace REstate.Engine
{
    public class REstateMachineFactory<TState, TInput>
        : IStateMachineFactory<TState, TInput>
    {
        private readonly IConnectorResolver<TState, TInput> _connectorResolver;
        private readonly IRepositoryContextFactory<TState, TInput> _repositoryContextFactory;
        private readonly ICartographer<TState, TInput> _cartographer;
        private readonly IEnumerable<IEventListener> _listeners;

        public REstateMachineFactory(
            IConnectorResolver<TState, TInput> connectorResolver,
            IRepositoryContextFactory<TState, TInput> repositoryContextFactory,
            ICartographer<TState, TInput> cartographer,
            IEnumerable<IEventListener> listeners)
        {
            _connectorResolver = connectorResolver;
            _repositoryContextFactory = repositoryContextFactory;
            _cartographer = cartographer;
            _listeners = listeners;
        }

        public IStateMachine<TState, TInput> ConstructFromSchematic(string machineId, ISchematic<TState, TInput> schematic, IReadOnlyDictionary<string, string> metadata)
        {
            if (schematic == null)
                throw new ArgumentNullException(nameof(schematic));

            var reStateMachine = new REstateMachine<TState, TInput>(_connectorResolver, _repositoryContextFactory, _cartographer, _listeners, machineId, schematic, metadata);

            return reStateMachine;
        }
    }
}