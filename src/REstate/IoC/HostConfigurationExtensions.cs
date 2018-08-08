using REstate.Engine.Connectors;

namespace REstate.IoC
{
    public static class HostConfigurationExtensions
    {
        public static void RegisterConnector<TConnector>(
            this IHostConfiguration hostConfiguration,
            IConnectorConfiguration connectorConfiguration = null,
            string registrationName = null) where TConnector : IConnector
        {
            hostConfiguration.RegisterComponent(new SingleConnectorComponent<TConnector>(connectorConfiguration, registrationName));
        }

        private class SingleConnectorComponent<TConnector>
            : IComponent
            where TConnector : IConnector
        {
            private readonly IConnectorConfiguration _connectorConfiguration;
            private readonly string _registrationName;

            public SingleConnectorComponent(IConnectorConfiguration connectorConfiguration, string registrationName = null)
            {
                _connectorConfiguration = connectorConfiguration;
                _registrationName = registrationName;
            }

            public void Register(IRegistrar registrar)
            {
                registrar.RegisterConnector<TConnector>(
                    _connectorConfiguration ?? new ConnectorConfiguration(TypeState.FromType(typeof(TConnector)).GetConnectorKey()), _registrationName);

            }
        }
    }
}
