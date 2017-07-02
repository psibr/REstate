using REstate.Configuration;

namespace REstate.Engine
{
    public interface IStateMachineFactory<TState>
    {
        IStateMachine<TState> ConstructFromConfiguration(string machineId, Schematic<TState> configuration);
    }
}
