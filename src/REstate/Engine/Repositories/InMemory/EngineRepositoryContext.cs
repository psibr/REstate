namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepositoryContext<TState, TInput>
        : IEngineRepositoryContext<TState, TInput>
    {
        public EngineRepositoryContext()
        {
            var repo = new EngineRepository<TState, TInput>();

            Schematics = repo;
            Machines = repo;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        public ISchematicRepository<TState, TInput> Schematics { get; }

        public IMachineRepository<TState, TInput> Machines { get; }
    }
}
