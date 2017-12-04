using System.Collections.Generic;
using REstate.Schematics;

namespace REstate.Engine
{
    public interface IStateMachineFactory<TState, TInput>
    {
        IStateMachine<TState, TInput> ConstructFromSchematic(
            string machineId, 
            ISchematic<TState, TInput> schematic,
            IReadOnlyDictionary<string, string> metadata);
    }
}
