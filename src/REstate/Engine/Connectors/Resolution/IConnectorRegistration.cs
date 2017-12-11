namespace REstate.Engine.Connectors.Resolution
{
    public interface IConnectorRegistration
    {
        IConnectorRegistration WithConfiguration<TConfiguration>(TConfiguration configuration) 
            where TConfiguration : class, IConnectorConfiguration;
    }
}