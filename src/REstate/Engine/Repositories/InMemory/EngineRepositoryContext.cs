using System;

namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepositoryContext<TState>
        : IEngineRepositoryContext<TState>
    {
        public EngineRepositoryContext()
        {
            var repo = new EngineRepository<TState>();

            Schematics = repo;
            Machines = repo;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public ISchematicRepository<TState> Schematics { get; }

        public IMachineRepository<TState> Machines { get; }
    }
}
