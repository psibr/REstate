﻿namespace REstate.Engine.Connectors
{
    /// <summary>
    /// A configuration object that can be associated with a connector.
    /// </summary>
    public class ConnectorConfiguration
        : IConnectorConfiguration
    {
        public ConnectorConfiguration(string identifier)
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
    }
}
