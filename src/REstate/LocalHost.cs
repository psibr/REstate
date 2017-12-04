namespace REstate
{
    /// <summary>
    /// An interface that provides the ability to scope an agent to strictly local execution
    /// </summary>
    public interface ILocalHost
    {
        /// <summary>
        /// Retrieves a StateEngine that executes state changes locally
        /// </summary>
        /// <returns>A local execution state engine</returns>
        ILocalStateEngine<TState, TInput> GetStateEngine<TState, TInput>();
    }

    /// <summary>
    /// A <see cref="IStateEngine&lt;TState, TInput&gt;" /> that executes state changes locally
    /// </summary>
    public interface ILocalStateEngine<TState, TInput>
        : IStateEngine<TState, TInput>
    {
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
