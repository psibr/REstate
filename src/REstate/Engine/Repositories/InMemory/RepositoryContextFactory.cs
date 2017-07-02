using System;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryRepositoryContextFactory<TState>
        : IRepositoryContextFactory<TState>
    {

        public InMemoryRepositoryContextFactory()
        {
        }

        public IEngineRepositoryContext<TState> OpenContext()
        {
            return new EngineRepositoryContext<TState>();
        }
    }
}
