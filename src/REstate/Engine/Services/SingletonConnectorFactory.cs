using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Services
{
    public class SingletonConnectorFactory 
        : IConnectorFactory
    {
        private readonly IConnector _connector;

        public SingletonConnectorFactory(IConnector connector, bool isActionConnector = true, bool isGuardConnector = false)
        {
            if (connector == null)
                throw new ArgumentNullException(nameof(connector));

            ConnectorKey = connector.ConnectorKey;
            _connector = connector;

            IsActionConnector = isActionConnector;
            IsGuardConnector = isGuardConnector;

        }

        public string ConnectorKey { get; }

        public bool IsActionConnector { get; }

        public bool IsGuardConnector { get; }

        public Task<IConnector> BuildConnectorAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_connector);
        }
    }
}
