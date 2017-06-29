using System;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryRepositoryContextFactory
        : IRepositoryContextFactory
    {
        private readonly StringSerializer _stringSerializer;

        public InMemoryRepositoryContextFactory(StringSerializer stringSerializer)
        {
            _stringSerializer = stringSerializer;
        }

        public IEngineRepositoryContext OpenContext()
        {
            return new EngineRepositoryContext(_stringSerializer);
        }
    }
}
