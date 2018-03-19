namespace REstate.Remote
{
    public interface IRemoteAgent
    {
        IRemoteStateEngine<TState, TInput> GetStateEngine<TState, TInput>();
    }

    public class RemoteAgent
        : IRemoteAgent
    {
        private readonly IAgent _agent;

        public RemoteAgent(IAgent agent)
        {
            _agent = agent;
        }

        public IRemoteStateEngine<TState, TInput> GetStateEngine<TState, TInput>() =>
            _agent.GetStateEngine<TState, TInput, IRemoteStateEngine<TState, TInput>>();
    }
}
