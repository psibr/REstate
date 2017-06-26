using REstate.Configuration;

namespace REstate.Engine
{
    public interface IStateMachineFactory
    {
        IStateMachine ConstructFromConfiguration(string machineId, Schematic configuration);
    }
}
