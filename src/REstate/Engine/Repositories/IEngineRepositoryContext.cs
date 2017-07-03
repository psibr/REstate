using System;

namespace REstate.Engine.Repositories
{
    public interface IEngineRepositoryContext<TState, TInput>
        : IDisposable
    {
        ISchematicRepository<TState, TInput> Schematics { get; }

        IMachineRepository<TState, TInput> Machines { get; }
    }
}
