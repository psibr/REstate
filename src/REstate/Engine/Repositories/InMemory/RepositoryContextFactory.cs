using System;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryRepositoryContextFactory<TState, TInput>
        : IRepositoryContextFactory<TState, TInput>
    {
        public IEngineRepositoryContext<TState, TInput> OpenContext()
        {
            return new EngineRepositoryContext<TState, TInput>();
        }
    }
}
