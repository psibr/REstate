using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Resolution
{
    public interface IConnectorResolver<TState, TInput>
    {
        IEntryConnector<TState, TInput> ResolveEntryConnector(ConnectorKey connectorKey);

        IBulkEntryConnector<TState, TInput> ResolveBulkEntryConnector(ConnectorKey connectorKey);

        IGuardianConnector<TState, TInput> ResolveGuardianConnector(ConnectorKey connectorKey);
    }

    /// <summary>
    /// The default entryConnector resoultion strategy that ignores versioning.
    /// </summary>
    public class DefaultConnectorResolver<TState, TInput>
        : IConnectorResolver<TState, TInput>
    {
        protected readonly ILookup<string, IEntryConnector<TState, TInput>> EntryConnectors;
        protected readonly ILookup<string, IBulkEntryConnector<TState, TInput>> BulkEntryConnectors;
        protected readonly ILookup<string, IGuardianConnector<TState, TInput>> GuardianConnectors;

        public DefaultConnectorResolver(
            IEnumerable<IEntryConnector<TState, TInput>> entryConnectors,
            IEnumerable<IGuardianConnector<TState, TInput>> guardianConnectors,
            IEnumerable<ConnectorTypeToIdentifierMapping> connectorTypeToIdentifierMappings)
        {
            var connectorStringComparer = StringComparer.OrdinalIgnoreCase;

            var mappings = connectorTypeToIdentifierMappings.ToList();

            var entryConnectorMappings = mappings
                .Join(inner: entryConnectors.Where(connector => connector != null),
                    outerKeySelector: mapping => 
                        mapping.ConnectorType,
                    innerKeySelector: connector => 
                        connector.GetType().IsConstructedGenericType 
                        ? connector.GetType().GetGenericTypeDefinition() 
                        : connector.GetType(),
                    resultSelector: (mapping, connector) => 
                        (mapping.Identifier, Connector: connector))
                .ToList();

            EntryConnectors = entryConnectorMappings
                .ToLookup(kvp => kvp.Identifier, kvp => kvp.Connector, connectorStringComparer);

            BulkEntryConnectors = entryConnectorMappings
                .Where(tuple => tuple.Connector.GetType()
                    .GetInterfaces()
                    .Where(i => i.IsConstructedGenericType)
                    .Select(i => i.GetGenericTypeDefinition())
                    .Any(i => i == typeof(IBulkEntryConnector<,>)))
                .Select(tuple => (tuple.Identifier, Connector: (IBulkEntryConnector<TState, TInput>)tuple.Connector))
                .ToLookup(tuple => tuple.Identifier, tuple => tuple.Connector, connectorStringComparer);

            GuardianConnectors = mappings
                .Join(inner: guardianConnectors.Where(connector => connector != null),
                    outerKeySelector: mapping => mapping.ConnectorType,
                    innerKeySelector: connector => connector.GetType().IsConstructedGenericType 
                        ? connector.GetType().GetGenericTypeDefinition() 
                        : connector.GetType(),
                    resultSelector: (mapping, connector) => (mapping.Identifier, Connector: connector))
                .ToLookup(kvp => kvp.Identifier, kvp => kvp.Connector, connectorStringComparer);
        }

        public virtual IEntryConnector<TState, TInput> ResolveEntryConnector(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            IEntryConnector<TState, TInput> entryConnector;
            try
            {
                entryConnector = EntryConnectors[connectorKey.Identifier].SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ConnectorResolutionException($"Multiple EntryConnector versions exists with matching connectorKey: \"{connectorKey.Identifier}\".");
            }

            if (entryConnector == null)
                throw new ConnectorResolutionException($"No EntryConnector exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return entryConnector;
        }

        public virtual IBulkEntryConnector<TState, TInput> ResolveBulkEntryConnector(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            IBulkEntryConnector<TState, TInput> bulkEntryConnector;
            try
            {
                bulkEntryConnector = BulkEntryConnectors[connectorKey.Identifier].SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ConnectorResolutionException($"Multiple BulkEntryConnector versions exists with matching connectorKey: \"{connectorKey.Identifier}\".");
            }

            if (bulkEntryConnector == null)
                throw new ConnectorResolutionException($"No BulkEntryConnector exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return bulkEntryConnector;
        }

        public IGuardianConnector<TState, TInput> ResolveGuardianConnector(ConnectorKey connectorKey)
        {
            if (connectorKey == null) throw new ArgumentNullException(nameof(connectorKey));

            IGuardianConnector<TState, TInput> guardianConnector;
            try
            {
                guardianConnector = GuardianConnectors[connectorKey.Identifier].SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ConnectorResolutionException($"Multiple GuardianConnector versions exists with matching connectorKey: \"{connectorKey.Identifier}\".");
            }

            if (guardianConnector == null)
                throw new ConnectorResolutionException($"No GuardianConnector exists matching connectorKey: \"{connectorKey.Identifier}\".");

            return guardianConnector;
        }
    }
}
