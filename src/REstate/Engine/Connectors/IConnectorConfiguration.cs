using REstate.Schematics;

namespace REstate.Engine.Connectors
{
    /// <summary>
    /// A configuration object that can be associated with a <see cref="IConnector"/>.
    /// </summary>
    public interface IConnectorConfiguration
    {
        /// <summary>
        /// A resolution string that can be used as a <see cref="ConnectorKey"/> in a <see cref="Schematic{TState,TInput}"/>.
        /// <para />
        /// Identifiers should ALWAYS be unique, overriding is NOT supported for safety/clarity reasons.
        /// </summary>
        /// <example>
        /// log info
        /// </example>
        string Identifier { get; }
    }
}
