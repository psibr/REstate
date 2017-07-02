namespace REstate.Engine.Services
{
    public interface IConnectorResolver<TState>
    {
        IConnector<TState> ResolveConnector(string connectorKey);
    }
}
