using REstate.Schematics;

namespace REstate.Engine.Connectors.Resolution
{
    /// <summary>
    /// Represents a registration of a connector.
    /// </summary>
    public interface IConnectorRegistration
    {
        /// <summary>
        /// Allows providing a configuration object to be associated with the 
        /// connector that at a minimum provides an Identifier to match with 
        /// a <see cref="ConnectorKey"/> in a <see cref="Schematic{TState,TInput}"/>.
        /// </summary>
        /// <typeparam name="TConfiguration">
        /// The implementation of <see cref="IConnectorConfiguration"/> to be associated
        /// with the registration.
        /// </typeparam>
        /// <param name="configuration">
        /// A configuration object that at a minimum provides an Identifier to match with 
        /// a <see cref="ConnectorKey"/> in a <see cref="Schematic{TState,TInput}"/>.
        /// </param>
        /// <returns>This same registration, so that more configurations can be associated.</returns>
        IConnectorRegistration WithConfiguration<TConfiguration>(TConfiguration configuration) 
            where TConfiguration : class, IConnectorConfiguration;
    }
}
