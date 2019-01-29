using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine.Connectors.Resolution;
using REstate.Engine.EventListeners;
using REstate.Engine.Repositories;
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

        public IStateMachine<TState, TInput> Construct(string machineId)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            var machineStatusStore = _repositoryContextFactory
                .GetMachineStatusStore(machineId);

            var restateMachine = new REstateMachine<TState, TInput>(
                _connectorResolver,
                machineStatusStore,
                _listeners,
                machineId);

            return restateMachine;
        }

        public IStateMachine<TState, TInput> ConstructPreloaded(string machineId, Schematic<TState, TInput> schematic, ReadOnlyDictionary<string, string> metadata)
        {
            var machineStatusStore = _repositoryContextFactory
                .GetMachineStatusStore(machineId);

            var restateMachine = new REstateMachine<TState, TInput>(
                _connectorResolver,
                machineStatusStore,
                _listeners,
                machineId,
                schematic,
                metadata);

            return restateMachine;
        }
    }
}
