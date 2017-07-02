using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Engine.Services
{
    public class DefaultConnectorResolver<TState>
        : IConnectorResolver<TState>
    {
        private readonly IDictionary<string, IConnector<TState>> _connectors;

        //public DefaultConnectorResolver(IEnumerable<IConnector<TState>> connectors)
        //{
        //    _connectors = connectors
        //        .ToDictionary(kvp => kvp.ConnectorKey, kvp => kvp);
        //}

        public DefaultConnectorResolver(IEnumerable<IConnector<TState>> connectors)
        {
            _connectors = connectors
                .ToDictionary(kvp => kvp.ConnectorKey, kvp => kvp);
        }

        public virtual IConnector<TState> ResolveConnector(string connectorKey)
        {
            if (!_connectors.TryGetValue(connectorKey, out IConnector<TState> connector))
                throw new ArgumentException($"No connector factory exists matching connectorKey: \"{connectorKey}\".", nameof(connectorKey));

            return connector;
        }
    }
}
