using REstate.Schematics;

namespace REstate.Engine.Connectors
{
    /// <summary>
    /// A configuration object that can be associated with a connector.
    /// </summary>
    public interface IConnectorConfiguration
    {
        /// <summary>
        /// A resolution string that can be used as a <see cref="ConnectorKey"/> in a Schematic.
        /// </summary>
        /// <example>
        /// log info
        /// </example>
        string Identifier { get; }
    }
}
