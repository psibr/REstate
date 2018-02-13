namespace REstate.Engine.Connectors
{
    public class ConnectorConfiguration
        : IConnectorConfiguration
    {
        public ConnectorConfiguration(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }
    }
}