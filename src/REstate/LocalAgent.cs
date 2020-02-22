using REstate.Engine;
using System;

namespace REstate
{
    /// <summary>
    /// An interface that provides the ability to scope an agent to strictly local execution
    /// </summary>
    public interface ILocalAgent
    {
        /// <summary>
        /// Retrieves a StateEngine that executes state changes locally
        /// </summary>
        /// <returns>A local execution state engine</returns>
        ILocalStateEngine<TState, TInput> GetStateEngine<TState, TInput>();
    }

    internal class LocalAgent
        : ILocalAgent
    {
        private readonly IAgent _agent;

        public LocalAgent(IAgent agent)
        {
            _agent = agent ?? 
                throw new ArgumentNullException(nameof(agent));
        }

        public ILocalStateEngine<TState, TInput> GetStateEngine<TState, TInput>() =>
            _agent.GetStateEngine<TState, TInput, ILocalStateEngine<TState, TInput>>();
    }
}
