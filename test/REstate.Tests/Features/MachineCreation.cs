using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Tests.Features.Context;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features
{
    [FeatureDescription(@"
In order to utilize REstate's functionality
As a developer
I want to create machines from schematics")]
    [ScenarioCategory("Machine Creation")]
    public class MachineCreation
        : FeatureFixture
    {

        [Scenario]
        public void A_simple_machine_can_be_created()
        {
            Runner.WithContext<REstateContext<string, string>>().RunScenario(
                _ => _.Given_a_new_host(),
                _ => _.Given_a_simple_schematic_with_an_initial_state_INITIALSTATE("Initial"),
                _ => _.When_a_Machine_is_created_from_a_Schematic(_.CurrentSchematic),
                _ => _.Then_the_Machine_is_created_successfully(_.CurrentMachine));
        }

        // Given a default host
        // And Given a schematic
        // When I create a Machine
        // Then the Machine is not null
        // Then the Machine has an Id

        // Given a default host
        // And Given a schematic
        // When I create a Machine with an Id
        // Then the Machine is not null
        // Then the Machine has the same Id

        #region Constructor
        public MachineCreation(ITestOutputHelper output)
            : base(output)
        {
        }
        #endregion
    }


}

