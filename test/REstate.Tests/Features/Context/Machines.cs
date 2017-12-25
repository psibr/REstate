using REstate.Engine;
using REstate.Schematics;
using Xunit;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext<TState, TInput>
    {
        public IStateMachine<TState, TInput> CurrentMachine { get; set; }

        public void When_a_Machine_is_created_from_a_Schematic(Schematic<TState, TInput> schematic)
        {
            CurrentMachine = CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .CreateMachineAsync(schematic).GetAwaiter().GetResult();
        }

        public void When_a_Machine_is_created_from_a_Schematic_with_a_MachineId(Schematic<TState, TInput> schematic, string machineId)
        {
            CurrentMachine = CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .CreateMachineAsync(schematic, machineId).GetAwaiter().GetResult();
        }

        public void Then_the_Machine_is_created_successfully(IStateMachine<TState, TInput> machine)
        {
            Assert.NotNull(machine);
            Assert.NotNull(machine.MachineId);
            Assert.NotEmpty(machine.MachineId);
        }

        public void Then_the_Machine_has_the_same_MachineId(string machineId)
        {
            Assert.Equal(machineId, CurrentMachine.MachineId);
        }
    }
}
