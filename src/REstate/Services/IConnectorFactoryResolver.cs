namespace REstate.Engine.Services
{
    public interface IConnectorFactoryResolver
    {
        IConnectorFactory ResolveConnectorFactory(string connectorKey);
    }
}
