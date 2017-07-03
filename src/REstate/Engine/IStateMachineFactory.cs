using REstate.Configuration;

namespace REstate.Engine
{
    public interface IStateMachineFactory<TState, TInput>
    {
        IStateMachine<TState, TInput> ConstructFromConfiguration(string machineId, Schematic<TState, TInput> configuration);
    }
}
