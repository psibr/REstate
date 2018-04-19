using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Resolution
{
    /// <summary>
    /// The default connector resoultion strategy that ignores versioning.
    /// </summary>
    public class DefaultConnectorResolver<TState, TInput>
        : IConnectorResolver<TState, TInput>
    {
        protected readonly IDictionary<string, IAction<TState, TInput>> Actions;
        protected readonly IDictionary<string, IBulkAction<TState, TInput>> BulkActions;
        protected readonly IDictionary<string, IPrecondition<TState, TInput>> Preconditions;

        public DefaultConnectorResolver(
            IEnumerable<IAction<TState, TInput>> actions,
            IEnumerable<IPrecondition<TState, TInput>> preconditions,
            IEnumerable<ConnectorTypeToIdentifierMapping> connectorTypeToIdentifierMappings)
        {
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
            Actions = actionAssociations
                .ToDictionary(
                    keySelector: association => association.Identifier,
                    elementSelector: association => association.Action,
                    comparer: connectorStringComparer);

            // Here we identify the subset of action mappings where the connector is ALSO a IBulkAction
            // of the same TState and TInput and convert those to a new dictionary for 
            // run-time resolutions by Identifier.
            BulkActions = actionAssociations
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
                    comparer: connectorStringComparer);

            // Find all mappings that match with the IEnumerable of IPreconditions that we have loaded
            // and convert those to a new dictionary for run-time resolutions by Identifier.
            Preconditions = mappings
                .Join(inner: preconditions.Where(precondition => precondition != null),
                    outerKeySelector: mapping => mapping.ConnectorType,
                    innerKeySelector: precondition => precondition.GetType().IsConstructedGenericType
                        ? precondition.GetType().GetGenericTypeDefinition()
                        : precondition.GetType(),
                    resultSelector: (mapping, precondition) => (mapping.Identifier, Precondition: precondition))
                .ToDictionary(
                    keySelector: association => association.Identifier,
                    elementSelector: association => association.Precondition,
                    comparer: connectorStringComparer);
        }

        /// <summary>
        /// Resolves an Action Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public virtual IAction<TState, TInput> ResolveAction(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            if (!Actions.TryGetValue(connectorKey.Identifier, out var action))
                throw new ConnectorResolutionException($"No Action exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return action;
        }

        /// <summary>
        /// Resolves a BulkAction Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public virtual IBulkAction<TState, TInput> ResolveBulkAction(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            if(!BulkActions.TryGetValue(connectorKey.Identifier, out var bulkAction))
                throw new ConnectorResolutionException($"No BulkAction exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return bulkAction;
        }

        /// <summary>
        /// Resolves a Precondition Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public IPrecondition<TState, TInput> ResolvePrecondition(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            if(!Preconditions.TryGetValue(connectorKey.Identifier, out var precondition))
                throw new ConnectorResolutionException($"No Precondition exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return precondition;
        }
    }
}