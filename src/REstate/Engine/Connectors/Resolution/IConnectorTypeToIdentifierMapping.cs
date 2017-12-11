using System;

namespace REstate.Engine.Connectors.Resolution
{
    public class ConnectorTypeToIdentifierMapping
    {
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