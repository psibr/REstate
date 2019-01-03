using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using REstate.IoC;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Resolution
{
    /// <summary>
    /// The default connector resoultion strategy that ignores versioning.
    /// </summary>
    public class DefaultConnectorResolver<TState, TInput>
        : IConnectorResolver<TState, TInput>
    {
        protected readonly ConcurrentDictionary<string, IAction<TState, TInput>> Actions;
        protected readonly ConcurrentDictionary<string, IBulkAction<TState, TInput>> BulkActions;
        protected readonly ConcurrentDictionary<string, IPrecondition<TState, TInput>> Preconditions;

        public DefaultConnectorResolver(
            IAgent agent,
            IEnumerable<IAction<TState, TInput>> actions,
            IEnumerable<IPrecondition<TState, TInput>> preconditions,
            IEnumerable<ConnectorTypeToIdentifierMapping> connectorTypeToIdentifierMappings)
        {
            Agent = agent;

            var connectorStringComparer = StringComparer.OrdinalIgnoreCase;

            // Convert the mappings to a list to avoid multiple-enumeration overhead.
            var mappings = connectorTypeToIdentifierMappings.ToList();

            // A note on Connector (Actions & Preconditions) to mapping associations.
            //
            // Connectors and the mappings match conditionally in one of two ways:
            // 1) If the IConnector implementation is a constructed generic such as LoggingAction<string, string>
            // then use the definition instead: LoggingAction<TState, TInput> to associate to a mapping.
            // 2) If the type is NOT a constructed generic such as MyAction then just use that.
            //
            // These checks are done to enable both scenarios respectively: 
            // 1) Common connectors like logging or enqueing where the type of the 
            // states and input aren't as critical to be known exactly, but can stil be used.
            // 2) Application-specific connectors such as a business process or system control event
            // where the types of the states and inputs would be expected to be a certain type. 
            // e.g.
            // where TState may be string, but TInput is a complex object that is specific to the domain.

            // Find all mappings that match with the IEnumerable of IActions that we have loaded.
            var actionAssociations = mappings
                .Join(inner: actions.Where(connector => connector != null),
                    outerKeySelector: mapping =>
                        mapping.ConnectorType,
                    innerKeySelector: action =>
                        action.GetType().IsConstructedGenericType
                            ? action.GetType().GetGenericTypeDefinition()
                            : action.GetType(),
                    resultSelector: (mapping, action) =>
                        (mapping.Identifier, Action: action))
                .ToList();

            // Convert those flat mappings to a dictionary for run-time resolutions by Identifier.
            Actions = new ConcurrentDictionary<string, IAction<TState, TInput>>(
                actionAssociations
                    .ToDictionary(
                        keySelector: association => association.Identifier,
                        elementSelector: association => association.Action,
                        comparer: connectorStringComparer));

            // Here we identify the subset of action mappings where the connector is ALSO a IBulkAction
            // of the same TState and TInput and convert those to a new dictionary for 
            // run-time resolutions by Identifier.
            BulkActions = new ConcurrentDictionary<string, IBulkAction<TState, TInput>>(
                actionAssociations
                    .Where(association => association.Action is IBulkAction<TState, TInput>)
                    .Select(association =>
                        (
                            association.Identifier,
                            BulkAction: (IBulkAction<TState, TInput>)association.Action
                        )
                    )
                    .ToDictionary(
                        keySelector: association => association.Identifier,
                        elementSelector: association => association.BulkAction,
                        comparer: connectorStringComparer));

            // Find all mappings that match with the IEnumerable of IPreconditions that we have loaded
            // and convert those to a new dictionary for run-time resolutions by Identifier.
            Preconditions = new ConcurrentDictionary<string, IPrecondition<TState, TInput>>(
                mappings
                    .Join(inner: preconditions.Where(precondition => precondition != null),
                        outerKeySelector: mapping => mapping.ConnectorType,
                        innerKeySelector: precondition => precondition.GetType().IsConstructedGenericType
                            ? precondition.GetType().GetGenericTypeDefinition()
                            : precondition.GetType(),
                        resultSelector: (mapping, precondition) => (mapping.Identifier, Precondition: precondition))
                    .ToDictionary(
                        keySelector: association => association.Identifier,
                        elementSelector: association => association.Precondition,
                        comparer: connectorStringComparer));
        }

        protected IAgent Agent { get; }

        /// <summary>
        /// Resolves an Action Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public virtual IAction<TState, TInput> ResolveAction(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            do
            {
                try
                {
                    if (!Actions.TryGetValue(connectorKey.Identifier, out var action))
                        throw new ConnectorResolutionException($"No Action exists matching connectorKey: \"{connectorKey.Identifier}\".");

                    return action;
                }
                catch (ConnectorResolutionException)
                {
                    var connectorType = Type.GetType(connectorKey.Identifier);

                    if (connectorType != null)
                    {
                        IAction<TState, TInput> action = null;

                        var resolutionExceptions = new List<Exception>(0);

                        for (var i = 0; i <= 1; i++)
                        {
                            try
                            {
                                action = (Agent.Configuration as HostConfiguration).Container
                                    .Resolve<IAction<TState, TInput>>(connectorKey.Identifier);
                            }
                            catch (Exception ex)
                            {
                                resolutionExceptions.Add(ex);

                                Agent.Configuration.Register(registrar =>
                                    registrar.RegisterConnector(connectorType)
                                        .WithConfiguration(new ConnectorConfiguration(connectorKey.Identifier)));
                            }
                        }

                        if(action == null)
                            throw new AggregateException(
                                message: "Action resolution failed even after best attempt re-register.", 
                                innerExceptions: resolutionExceptions);

                        Actions.TryAdd(connectorKey.Identifier, action);

                        continue;
                    }

                    throw;
                }
            }
            while (true);
        }

        /// <summary>
        /// Resolves a BulkAction Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public virtual IBulkAction<TState, TInput> ResolveBulkAction(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            do
            {
                try
                {
                    if (!BulkActions.TryGetValue(connectorKey.Identifier, out var bulkAction))
                        throw new ConnectorResolutionException($"No BulkAction exists matching connectorKey: \"{connectorKey.Identifier}\".");

                    return bulkAction;
                }
                catch (ConnectorResolutionException)
                {
                    var connectorType = Type.GetType(connectorKey.Identifier);

                    if (connectorType != null)
                    {
                        Agent.Configuration.Register(registrar =>
                            registrar.RegisterConnector(connectorType)
                                .WithConfiguration(new ConnectorConfiguration(connectorKey.Identifier)));

                        var bulkAction = (Agent.Configuration as HostConfiguration).Container
                            .Resolve<IBulkAction<TState, TInput>>(connectorKey.Identifier);

                        BulkActions.TryAdd(connectorKey.Identifier, bulkAction);

                        continue;
                    }

                    throw;
                }
            }
            while (true);
        }

        /// <summary>
        /// Resolves a Precondition Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public virtual IPrecondition<TState, TInput> ResolvePrecondition(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            do
            {
                try
                {
                    if (!Preconditions.TryGetValue(connectorKey.Identifier, out var precondition))
                        throw new ConnectorResolutionException($"No Precondition exists matching connectorKey: \"{connectorKey.Identifier}\".");

                    return precondition;
                }
                catch (ConnectorResolutionException)
                {
                    var connectorType = Type.GetType(connectorKey.Identifier);

                    if (connectorType != null)
                    {
                        Agent.Configuration.Register(registrar =>
                            registrar.RegisterConnector(connectorType)
                                .WithConfiguration(new ConnectorConfiguration(connectorKey.Identifier)));

                        var precondition = (Agent.Configuration as HostConfiguration).Container
                            .Resolve<IPrecondition<TState, TInput>>(connectorKey.Identifier);

                        Preconditions.TryAdd(connectorKey.Identifier, precondition);

                        continue;
                    }

                    throw;
                }
            }
            while (true);
        }
    }
}