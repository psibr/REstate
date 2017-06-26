using REstate.Engine.Services;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace REstate.Services
{
    public class SingletonConnectorFactory 
        : IConnectorFactory
    {
        private IConnector _connector;

        public SingletonConnectorFactory(IConnector connector, bool isActionConnector = true, bool isGuardConnector = false)
        {
            if (connector == null)
                throw new ArgumentNullException(nameof(connector));

            ConnectorKey = connector.ConnectorKey;
            _connector = connector;
        }

        public string ConnectorKey { get; private set; }

        public bool IsActionConnector { get; private set; }

        public bool IsGuardConnector { get; private set; }

        public Task<IConnector> BuildConnectorAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_connector);
        }
    }
}
