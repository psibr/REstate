using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Resolution
{
    /// <summary>
    /// Implementations resolve connectors from a given<see cref="ConnectorKey"/>.
    /// </summary>
    public interface IConnectorResolver<TState, TInput>
    {
        /// <summary>
        /// Resolves an Action Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        IAction<TState, TInput> ResolveAction(ConnectorKey connectorKey);

        /// <summary>
        /// Resolves a BlukAction Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        IBulkAction<TState, TInput> ResolveBulkAction(ConnectorKey connectorKey);

        /// <summary>
        /// Resolves a Precondition Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        IPrecondition<TState, TInput> ResolvePrecondition(ConnectorKey connectorKey);
    }

    /// <summary>
    /// The default connector resoultion strategy that ignores versioning.
    /// </summary>
    public class DefaultConnectorResolver<TState, TInput>
        : IConnectorResolver<TState, TInput>
    {
        protected readonly ILookup<string, IAction<TState, TInput>> Actions;
        protected readonly ILookup<string, IBulkAction<TState, TInput>> BulkActions;
        protected readonly ILookup<string, IPrecondition<TState, TInput>> Preconditions;

        public DefaultConnectorResolver(
            IEnumerable<IAction<TState, TInput>> actions,
            IEnumerable<IPrecondition<TState, TInput>> preconditions,
            IEnumerable<ConnectorTypeToIdentifierMapping> connectorTypeToIdentifierMappings)
        {
            var connectorStringComparer = StringComparer.OrdinalIgnoreCase;

            var mappings = connectorTypeToIdentifierMappings.ToList();

            var actionMappings = mappings
                .Join(inner: actions.Where(connector => connector != null),
                    outerKeySelector: mapping => 
                        mapping.ConnectorType,
                    innerKeySelector: connector => 
                        connector.GetType().IsConstructedGenericType 
                        ? connector.GetType().GetGenericTypeDefinition() 
                        : connector.GetType(),
                    resultSelector: (mapping, connector) => 
                        (mapping.Identifier, Connector: connector))
                .ToList();

            Actions = actionMappings
                .ToLookup(kvp => kvp.Identifier, kvp => kvp.Connector, connectorStringComparer);

            BulkActions = actionMappings
                .Where(tuple => tuple.Connector.GetType()
                    .GetInterfaces()
                    .Where(i => i.IsConstructedGenericType)
                    .Select(i => i.GetGenericTypeDefinition())
                    .Any(i => i == typeof(IBulkAction<,>)))
                .Select(tuple => (tuple.Identifier, Connector: (IBulkAction<TState, TInput>)tuple.Connector))
                .ToLookup(tuple => tuple.Identifier, tuple => tuple.Connector, connectorStringComparer);

            Preconditions = mappings
                .Join(inner: preconditions.Where(connector => connector != null),
                    outerKeySelector: mapping => mapping.ConnectorType,
                    innerKeySelector: connector => connector.GetType().IsConstructedGenericType 
                        ? connector.GetType().GetGenericTypeDefinition() 
                        : connector.GetType(),
                    resultSelector: (mapping, connector) => (mapping.Identifier, Connector: connector))
                .ToLookup(kvp => kvp.Identifier, kvp => kvp.Connector, connectorStringComparer);
        }

        /// <summary>
        /// Resolves an Action Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public virtual IAction<TState, TInput> ResolveAction(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            IAction<TState, TInput> action;
            try
            {
                action = Actions[connectorKey.Identifier].SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ConnectorResolutionException($"Multiple Action versions exists with matching connectorKey: \"{connectorKey.Identifier}\".");
            }

            if (action == null)
                throw new ConnectorResolutionException($"No Action exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return action;
        }

        /// <summary>
        /// Resolves a BlukAction Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public virtual IBulkAction<TState, TInput> ResolveBulkAction(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            IBulkAction<TState, TInput> bulkAction;
            try
            {
                bulkAction = BulkActions[connectorKey.Identifier].SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ConnectorResolutionException($"Multiple BulkAction versions exists with matching connectorKey: \"{connectorKey.Identifier}\".");
            }

            if (bulkAction == null)
                throw new ConnectorResolutionException($"No BulkAction exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return bulkAction;
        }

        /// <summary>
        /// Resolves a Precondition Connector, given a <see cref="ConnectorKey"/>.
        /// </summary>
        public IPrecondition<TState, TInput> ResolvePrecondition(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            IPrecondition<TState, TInput> precondition;
            try
            {
                precondition = Preconditions[connectorKey.Identifier].SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ConnectorResolutionException($"Multiple Procondition versions exists with matching connectorKey: \"{connectorKey.Identifier}\".");
            }

            if (precondition == null)
                throw new ConnectorResolutionException($"No Precondition exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return precondition;
        }
    }
}
