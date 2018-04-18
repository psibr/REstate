using System;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Resolution
{
    /// <summary>
    /// A simple mapping class that allows a <see cref="IConnectorResolver{TState,TInput}"/> to 
    /// locate a type to resolve based on <see cref="ConnectorKey"/> 
    /// in a <see cref="Schematic{TState,TInput}"/> using the Identifier 
    /// provided at <see cref="IConnector"/> registration in a <see cref="IConnectorConfiguration"/>.
    /// </summary>
    /// <remarks>
    /// This is preferrable to simply registering <see cref="Type"/>s with the Identifiers 
    /// as the registration key as this object lets us use the identifier to inform our resolution process.
    /// </remarks>
    public class ConnectorTypeToIdentifierMapping
    {
        /// <summary>
        /// Creates a simple mapping class that allows a <see cref="IConnectorResolver{TState,TInput}"/> to 
        /// locate a <see cref="Type"/> to resolve based on <see cref="ConnectorKey"/> 
        /// in a <see cref="Schematic{TState,TInput}"/> using the Identifier 
        /// provided at <see cref="IConnector"/> registration in a <see cref="IConnectorConfiguration"/>.
        /// </summary>
        /// <param name="connectorType">
        /// The generic type definition of the connector type.
        /// </param>
        /// <param name="identifier">
        /// The identifier used when registering the <see cref="IConnector"/>. 
        /// This should also match what is provided for <see cref="ConnectorKey"/>
        /// </param>
        public ConnectorTypeToIdentifierMapping(Type connectorType, string identifier)
        {
            ConnectorType = connectorType;
            Identifier = identifier;
        }

        /// <summary>
        /// The generic type definition of the connector type.
        /// </summary>
        public Type ConnectorType { get; }

        /// <summary>
        /// The identifier registered.
        /// </summary>
        public string Identifier { get; }
    }
}
