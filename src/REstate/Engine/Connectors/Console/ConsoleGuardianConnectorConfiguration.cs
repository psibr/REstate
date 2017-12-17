namespace REstate.Engine.Connectors.Console
{
    public class ConsoleGuardianConnectorConfiguration
        : IConnectorConfiguration
    {
        public ConsoleGuardianConnectorConfiguration(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public ConsoleReadMode Mode { get; set; } = ConsoleReadMode.ReadLine;
    }
}