using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Engine.Services.ConnectorResolvers
{
    public interface IConnectorResolver<TState, TInput>
    {
        IConnector<TState, TInput> ResolveConnector(ConnectorKey key);
    }

    /// <summary>
    /// The default connector resoultion strategy that ignores versioning.
    /// </summary>
    public class DefaultConnectorResolver<TState, TInput>
        : IConnectorResolver<TState, TInput>
    {
        protected readonly ILookup<ConnectorKey, IConnector<TState, TInput>> Connectors;

        public DefaultConnectorResolver(IEnumerable<IConnector<TState, TInput>> connectors)
        {
            Connectors = connectors.ToLookup(kvp => kvp.Key, kvp => kvp, new ConnectorKeyNameEqualityComparer());
        }

        public virtual IConnector<TState, TInput> ResolveConnector(ConnectorKey key)
        {
            IConnector<TState, TInput> connector;
            try
            {
                connector = Connectors[key].SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException($"Multiple connector versions exists with matching connectorKey: \"{key.Name}\".", nameof(key.Name));
            }

            if (connector == null)
                throw new ArgumentException($"No connector exists matching connectorKey: \"{key.Name}\".", nameof(key.Name));

            return connector;
        }
    }
}
