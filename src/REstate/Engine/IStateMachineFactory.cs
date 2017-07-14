using REstate.Configuration;

namespace REstate.Engine
{
    public interface IStateMachineFactory<TState, TInput>
    {
        IStateMachine<TState, TInput> ConstructFromSchematic(string machineId, Schematic<TState, TInput> configuration);
    }
}
