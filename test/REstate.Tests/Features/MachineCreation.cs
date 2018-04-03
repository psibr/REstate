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
In order to utilize REstate's functionality
As a developer
I want to create machines from schematics")]
    [ScenarioCategory("Machine Creation")]
    public abstract class MachineCreationScenarios<TContext>
        : REstateFeature<TContext>
        where TContext : REstateContext<string, string>, new()
    {
        [Scenario]
        public async Task A_Machine_can_be_created_from_a_Schematic()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.When_a_Machine_is_created_from_a_Schematic(_.CurrentSchematic),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machine_is_valid(_.CurrentMachine));
        }

        [Scenario]
        public async Task A_Machine_can_be_created_from_a_SchematicName()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.Given_a_Schematic_is_stored(_.CurrentSchematic),
                _ => _.When_a_Machine_is_created_from_a_SchematicName(_.CurrentSchematic.SchematicName),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machine_is_valid(_.CurrentMachine));
        }

        [Scenario]
        public async Task A_Machine_can_be_created_from_a_Schematic_with_a_predefined_MachineId()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;
            var machineId = uniqueId;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.When_a_Machine_is_created_from_a_Schematic_with_a_predefined_MachineId(_.CurrentSchematic, machineId),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machine_is_valid(_.CurrentMachine),
                _ => _.Then_the_MachineId_is_MACHINEID(_.CurrentMachine, machineId));
        }

        [Scenario]
        public async Task A_Machine_can_be_created_from_a_SchematicName_with_a_predefined_MachineId()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;
            var machineId = uniqueId;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.Given_a_Schematic_is_stored(_.CurrentSchematic),
                _ => _.When_a_Machine_is_created_from_a_SchematicName_with_a_predefined_MachineId(_.CurrentSchematic.SchematicName, machineId),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_the_Machine_is_valid(_.CurrentMachine),
                _ => _.Then_the_MachineId_is_MACHINEID(_.CurrentMachine, machineId));
        }

        [Scenario]
        public async Task Machines_can_be_bulk_created_from_a_SchematicName()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.Given_a_Schematic_is_stored(_.CurrentSchematic),
                _ => _.When_MACHINECOUNT_Machines_are_bulk_created_from_a_SchematicName(_.CurrentSchematic.SchematicName, 5),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_MACHINECOUNT_Machines_were_created(_.BulkCreatedMachines, 5),
                _ => _.Then_the_Machines_created_are_valid(_.BulkCreatedMachines));
        }

        [Scenario]
        public async Task Machines_can_be_bulk_created_from_a_Schematic()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.When_MACHINECOUNT_Machines_are_bulk_created_from_a_Schematic(_.CurrentSchematic, 5),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_MACHINECOUNT_Machines_were_created(_.BulkCreatedMachines, 5),
                _ => _.Then_the_Machines_created_are_valid(_.BulkCreatedMachines));
        }
    }

    public class MachineCreation
    : MachineCreationScenarios<REstateContext<string, string>>
    {

    }
}

