namespace REstate.Engine.Connectors.Resolution
{
    /// <summary>
    /// Represents a registration of a connector.
    /// </summary>
    public interface IConnectorRegistration
    {
        /// <summary>
        /// Allows providing a configuration object to be associated with the connector.
        /// </summary>
        /// <typeparam name="TConfiguration"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        IConnectorRegistration WithConfiguration<TConfiguration>(TConfiguration configuration) 
            where TConfiguration : class, IConnectorConfiguration;
    }
}
