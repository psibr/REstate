using System.Reflection;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Remote.Tests.Features.Context;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace REstate.Remote.Tests.Features
{
    [FeatureDescription(@"
In order to support cloud scaling
As a developer
I want to create Machines from Schematics on a remote server")]
    [ScenarioCategory("Machine Creation")]
    [ScenarioCategory("Remote")]
    public class MachineCreation
        : FeatureFixture
    {
        [Scenario]
        public void A_machine_can_be_created_from_a_Schematic()
        {
            var schematicName = MethodBase.GetCurrentMethod().Name;

            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_REstate_gRPC_Server_running_on_the_default_endpoint(),
                _ => _.Given_the_default_agent_is_gRPC_remote_on_default_endpoint(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.When_a_Machine_is_created_from_a_Schematic(_.CurrentSchematic),
                _ => _.Then_the_Machine_is_created_successfully(_.CurrentMachine));
        }

        [Scenario]
        public void A_machine_can_be_created_from_a_SchematicName()
        {
            var schematicName = MethodBase.GetCurrentMethod().Name;

            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_REstate_gRPC_Server_running_on_the_default_endpoint(),
                _ => _.Given_the_default_agent_is_gRPC_remote_on_default_endpoint(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.Given_a_Schematic_is_stored(_.CurrentSchematic),
                _ => _.When_a_Machine_is_created_from_a_SchematicName(_.CurrentSchematic.SchematicName),
                _ => _.Then_the_Machine_is_created_successfully(_.CurrentMachine));
        }

        [Scenario]
        public void A_machine_can_be_created_from_a_Schematic_with_a_predefined_MachineId()
        {
            var schematicName = MethodBase.GetCurrentMethod().Name;
            var machineId = MethodBase.GetCurrentMethod().Name;

            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_REstate_gRPC_Server_running_on_the_default_endpoint(),
                _ => _.Given_the_default_agent_is_gRPC_remote_on_default_endpoint(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.When_a_Machine_is_created_from_a_Schematic_with_a_predefined_MachineId(_.CurrentSchematic, machineId),
                _ => _.Then_the_Machine_is_created_successfully(_.CurrentMachine),
                _=> _.Then_the_MachineId_is_MACHINEID(_.CurrentMachine, machineId));
        }

        [Scenario]
        public void A_machine_can_be_created_from_a_SchematicName_with_a_predefined_MachineId()
        {
            var schematicName = MethodBase.GetCurrentMethod().Name;
            var machineId = MethodBase.GetCurrentMethod().Name;

            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_REstate_gRPC_Server_running_on_the_default_endpoint(),
                _ => _.Given_the_default_agent_is_gRPC_remote_on_default_endpoint(),
                _ => _.Given_a_Schematic_with_an_initial_state_INITIALSTATE(schematicName, "Initial"),
                _ => _.Given_a_Schematic_is_stored(_.CurrentSchematic),
                _ => _.When_a_Machine_is_created_from_a_SchematicName_with_a_predefined_MachineId(_.CurrentSchematic.SchematicName, machineId),
                _ => _.Then_the_Machine_is_created_successfully(_.CurrentMachine),
                _ => _.Then_the_MachineId_is_MACHINEID(_.CurrentMachine, machineId));
        }

        #region Constructor
        public MachineCreation(ITestOutputHelper output)
            : base(output)
        {
        }
        #endregion
    }


}

