namespace REstate.Engine.Connectors.Logger
{
    public class LoggerEntryConnectorConfiguration
        : IConnectorConfiguration
    {
        public LoggerEntryConnectorConfiguration(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public LogLevel LogLevel { get; set; } = LogLevel.Info;
    }
}