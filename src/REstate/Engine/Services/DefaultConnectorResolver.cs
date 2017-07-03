using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Engine.Services
{
    public class DefaultConnectorResolver<TState, TInput>
        : IConnectorResolver<TState, TInput>
    {
        private readonly IDictionary<string, IConnector<TState, TInput>> _connectors;

        //public DefaultConnectorResolver(IEnumerable<IConnector<TState>> connectors)
        //{
        //    _connectors = connectors
        //        .ToDictionary(kvp => kvp.ConnectorKey, kvp => kvp);
        //}

        public DefaultConnectorResolver(IEnumerable<IConnector<TState, TInput>> connectors)
        {
            _connectors = connectors
                .ToDictionary(kvp => kvp.ConnectorKey, kvp => kvp);
        }

        public virtual IConnector<TState, TInput> ResolveConnector(string connectorKey)
        {
            if (!_connectors.TryGetValue(connectorKey, out IConnector<TState, TInput> connector))
                throw new ArgumentException($"No connector factory exists matching connectorKey: \"{connectorKey}\".", nameof(connectorKey));

            return connector;
        }
    }
}
