using System;

namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepositoryContext
        : IEngineRepositoryContext
    {
        public EngineRepositoryContext(StringSerializer stringSerializer)
        {
            var repo = new EngineRepository(stringSerializer);

            Schematics = repo;
            Machines = repo;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public ISchematicRepository Schematics { get; }

        public IMachineRepository Machines { get; }
    }
}
