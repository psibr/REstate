namespace REstate.Engine.Services
{
    public interface IConnectorResolver<TState, TInput>
    {
        IConnector<TState, TInput> ResolveConnector(string connectorKey);
    }
}
