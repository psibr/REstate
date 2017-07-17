namespace REstate
{
    public interface ILocalHost
    {
        ILocalStateEngine<TState, TInput> GetStateEngine<TState, TInput>();
    }

    internal class LocalHost
        : ILocalHost
    {
        private readonly IAgent _agent;

        public LocalHost(IAgent agent)
        {
            _agent = agent;
        }

        public ILocalStateEngine<TState, TInput> GetStateEngine<TState, TInput>() =>
            _agent.GetStateEngine<TState, TInput, ILocalStateEngine<TState, TInput>>();
    }
}
