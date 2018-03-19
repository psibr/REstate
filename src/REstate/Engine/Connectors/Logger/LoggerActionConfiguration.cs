namespace REstate.Engine.Connectors.Logger
{
    public class LoggingActionConfiguration
        : IConnectorConfiguration
    {
        public LoggingActionConfiguration(string identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        /// A resolution string that can be used as a <see cref="ConnectorKey"/> in a Schematic.
        /// </summary>
        /// <example>
        /// log info
        /// </example>
        public string Identifier { get; }

        public LogLevel LogLevel { get; set; } = LogLevel.Info;
    }
}
