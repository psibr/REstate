using System;

namespace REstate.Engine.Repositories
{
    public interface IEngineRepositoryContext
        : IDisposable
    {
        ISchematicRepository Schematics { get; }

        IMachineRepository Machines { get; }
    }
}
