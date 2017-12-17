namespace REstate.Engine.Connectors.Console
{
    public class ConsoleEntryConnectorConfiguration
        : IConnectorConfiguration
    {
        public ConsoleEntryConnectorConfiguration(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public ConsoleWriteMode Mode { get; set; } = ConsoleWriteMode.WriteLine;
    }
}