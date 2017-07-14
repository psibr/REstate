namespace REstate.Remote
{
    public interface IRemoteHost
    {
        IRemoteStateEngine<TState, TInput> GetStateEngine<TState, TInput>();
    }

    public class RemoteHost 
        : IRemoteHost
    {
        private readonly IAgent _agent;

        public RemoteHost(IAgent agent)
        {
            _agent = agent;
        }

        public IRemoteStateEngine<TState, TInput> GetStateEngine<TState, TInput>() =>
            _agent.GetStateEngine<TState, TInput, IRemoteStateEngine<TState, TInput>>();
    }
}