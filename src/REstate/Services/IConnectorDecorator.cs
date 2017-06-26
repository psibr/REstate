namespace REstate.Engine.Services
{
    public interface IConnectorDecorator
    {
        IConnector Decorate(IConnector connector);
    }
}
