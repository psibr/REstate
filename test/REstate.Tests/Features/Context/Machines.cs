using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using REstate.Engine;
using REstate.Schematics;
using Xunit;
using Xunit.Sdk;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext<TState, TInput>
    {
        public IStateMachine<TState, TInput> CurrentMachine { get; set; }

        public List<IStateMachine<TState, TInput>> BulkCreatedMachines { get; set; }

        #region GIVEN
        public async Task Given_a_Machine_exists_with_MachineId_MACHINEID(Schematic<TState, TInput> schematic, string machineId)
        {
            CurrentMachine = await CurrentHost.Agent()
                .GetStateEngine<TState, TInput>()
                .CreateMachineAsync(schematic, machineId);
        }
        #endregion

        #region WHEN
        public async Task When_a_Machine_is_created_from_a_Schematic(Schematic<TState, TInput> schematic)
        {
            try
            {
                CurrentMachine = await CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .CreateMachineAsync(schematic);
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public async Task When_a_Machine_is_created_from_a_SchematicName(string schematicName)
        {
            try
            {
                CurrentMachine = await CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .CreateMachineAsync(schematicName);
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }

        }

        public async Task When_MACHINECOUNT_Machines_are_bulk_created_from_a_SchematicName(string schematicName, int machineCount)
        {
            try
            {
                BulkCreatedMachines = (await CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .BulkCreateMachinesAsync(schematicName, Enumerable.Repeat(new Dictionary<string, string>(0), machineCount))).ToList();
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public async Task When_MACHINECOUNT_Machines_are_bulk_created_from_a_Schematic(ISchematic<TState, TInput> schematic, int machineCount)
        {
            try
            {
                BulkCreatedMachines = (await CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .BulkCreateMachinesAsync(schematic, Enumerable.Repeat(new Dictionary<string, string>(0), machineCount))).ToList();
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public async Task When_a_Machine_is_created_from_a_Schematic_with_a_predefined_MachineId(
            Schematic<TState, TInput> schematic, string machineId)
        {
            try
            {
                CurrentMachine = await CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .CreateMachineAsync(schematic, machineId);
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public async Task When_a_Machine_is_created_from_a_SchematicName_with_a_predefined_MachineId(
            string schematicName, string machineId)
        {
            try
            {
                CurrentMachine = await CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .CreateMachineAsync(schematicName, machineId);
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public async Task When_a_Machine_is_retrieved_with_MachineId_MACHINEID(string machineId)
        {
            try
            {
                CurrentMachine = null;
                CurrentMachine = await CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .GetMachineAsync(machineId);
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }

        public async Task When_a_Machine_is_deleted_with_MachineId_MACHINEID(string machineId)
        {
            try
            {
                await CurrentHost.Agent()
                    .GetStateEngine<TState, TInput>()
                    .DeleteMachineAsync(machineId);
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }
        }
        #endregion
        
        #region THEN
        public Task Then_the_Machine_is_valid(IStateMachine<TState, TInput> machine)
        {
            Assert.NotNull(machine);
            Assert.NotNull(machine.MachineId);
            Assert.NotEmpty(machine.MachineId);

            return Task.CompletedTask;
        }

        public Task Then_MACHINECOUNT_Machines_were_created(
            List<IStateMachine<TState, TInput>> bulkCreatedMachines, 
            int machineCount)
        {
            Assert.NotNull(bulkCreatedMachines);
            Assert.Equal(machineCount, bulkCreatedMachines.Count);

            return Task.CompletedTask;
        }

        public Task Then_the_Machines_created_are_valid(List<IStateMachine<TState, TInput>> bulkCreatedMachines)
        {
            bulkCreatedMachines.ForEach(machine =>
            {
                Assert.NotNull(machine);
                Assert.NotNull(machine.MachineId);
                Assert.NotEmpty(machine.MachineId);
            });

            return Task.CompletedTask;
        }

        public Task Then_the_MachineId_is_MACHINEID(IStateMachine<TState, TInput> machine, string machineId)
        {
            Assert.Equal(machineId, CurrentMachine.MachineId);

            return Task.CompletedTask;
        }

        public Task Then_MachineDoesNotExistException_is_thrown()
        {
            Assert.NotNull(CurrentException);
            Assert.IsType<MachineDoesNotExistException>(CurrentException);

            return Task.CompletedTask;
        }
        #endregion
    }
}
