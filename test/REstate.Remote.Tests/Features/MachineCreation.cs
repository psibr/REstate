using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Remote.Tests.Features.Context;
using REstate.Tests.Features.Context;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace REstate.Remote.Tests.Features
{
    [FeatureDescription(@"
In order to utilize REstate's functionality
As a developer
I want to create machines from schematics on remote hosts")]
    [ScenarioCategory("Machine Creation")]
    [ScenarioCategory("Remote")]
    public class MachineCreation
        : FeatureFixture
    {

        [Scenario]
        public void A_machine_can_be_created()
        {
            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_REstate_RemoteServer_on_default_endpoint(),
                _ => _.Given_RemoteAgent_is_default_on_default_endpoint(),
                _ => _.Given_a_simple_schematic_with_an_initial_state_INITIALSTATE("Initial"),
                _ => _.When_a_Machine_is_created_from_a_Schematic(_.CurrentSchematic),
                _ => _.Then_the_Machine_is_created_successfully(_.CurrentMachine));
        }

        [Scenario]
        public void A_machine_can_be_created_with_a_provided_MachineId()
        {
            Runner.WithContext<REstateRemoteContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_REstate_RemoteServer_on_default_endpoint(),
                _ => _.Given_RemoteAgent_is_default_on_default_endpoint(),
                _ => _.Given_a_simple_schematic_with_an_initial_state_INITIALSTATE("Initial"),
                _ => _.When_a_Machine_is_created_from_a_Schematic_with_a_MachineId(_.CurrentSchematic, "123456789"),
                _ => _.Then_the_Machine_is_created_successfully(_.CurrentMachine),
                _ => _.Then_the_Machine_has_the_same_MachineId("123456789"));
        }

        #region Constructor
        public MachineCreation(ITestOutputHelper output)
            : base(output)
        {
        }
        #endregion
    }


}

