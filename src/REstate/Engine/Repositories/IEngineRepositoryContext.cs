using System;

namespace REstate.Engine.Repositories
{
    public interface IEngineRepositoryContext<TState>
        : IDisposable
    {
        ISchematicRepository<TState> Schematics { get; }

        IMachineRepository<TState> Machines { get; }
    }
}
