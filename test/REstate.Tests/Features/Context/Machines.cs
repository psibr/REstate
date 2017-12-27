using System;
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
            try
            { 
            CurrentMachine = CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .CreateMachineAsync(schematic).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public void When_a_Machine_is_created_from_a_SchematicName(string schematicName)
        {
            try
            {
                CurrentMachine = CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .CreateMachineAsync(schematicName).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }

        }

        public void When_a_Machine_is_created_from_a_Schematic_with_a_predefined_MachineId(
            Schematic<TState, TInput> schematic, string machineId)
        {
            try
            {
                CurrentMachine = CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .CreateMachineAsync(schematic, machineId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public void When_a_Machine_is_created_from_a_SchematicName_with_a_predefined_MachineId(
            string schematicName, string machineId)
        {
            try
            {
                CurrentMachine = CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .CreateMachineAsync(schematicName, machineId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public void Then_the_Machine_is_valid(IStateMachine<TState, TInput> machine)
        {
            Assert.NotNull(machine);
            Assert.NotNull(machine.MachineId);
            Assert.NotEmpty(machine.MachineId);
        }

        public void Then_the_MachineId_is_MACHINEID(IStateMachine<TState, TInput> machine, string machineId)
        {
            Assert.Equal(machineId, CurrentMachine.MachineId);
        }

        public void Given_a_Machine_exists_with_MachineId_MACHINEID(Schematic<TState, TInput> schematic, string machineId)
        {
            CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .CreateMachineAsync(schematic, machineId).GetAwaiter().GetResult();
        }

        public void When_a_Machine_is_retrieved_with_MachineId_MACHINEID(string machineId)
        {
            try
            {
                CurrentMachine = CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .GetMachineAsync(machineId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public void Then_MachineDoesNotExistException_is_thrown()
        {
            Assert.NotNull(CurrentException);
            Assert.IsType<MachineDoesNotExistException>(CurrentException);
        }
    }
}
