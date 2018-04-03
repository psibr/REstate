using System;
using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Tests.Features.Context;

// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features
{
    [FeatureDescription(@"
In order to remove previous executions or stop machines
As a developer
I want to delete previously created machines")]
    [ScenarioCategory("Machine Deletion")]
    public abstract class MachineDeletionScenarios<TContext>
        : REstateFeature<TContext>
        where TContext : REstateContext<string, string>, new()
    {
        [Scenario]
        public async Task A_Machine_that_is_deleted_no_longer_exists()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;
            var machineId = uniqueId;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.Given_a_Machine_exists_with_MachineId_MACHINEID(_.CurrentSchematic, machineId),
                _ => _.When_a_Machine_is_deleted_with_MachineId_MACHINEID(machineId),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.When_a_Machine_is_retrieved_with_MachineId_MACHINEID(machineId),
                _ => _.Then_MachineDoesNotExistException_is_thrown());
        }
    }
}
