using System;
using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using NaturalSchematicExamples;
using REstate.Tests.Features.Context;

namespace REstate.Tests.Features
{
    [FeatureDescription(@"
In order to support a rich integrated development experience
As a developer,
I want to be able to activate transitions on machines using the Natural syntax.")]
    [ScenarioCategory("NaturalMachineTransitions")]
    [ScenarioCategory("Machines")]
    public abstract class NaturalMachineTransitionScenarios<TContext>
        : REstateFeature<TContext>
        where TContext : REstateNaturalContext, new()
    {
        [Scenario]
        public async Task A_NaturalMachine_can_transition_to_another_state_with_an_automated_transition()
        {
            var uniqueId = Guid.NewGuid().ToString();

            await Runner.WithContext(Context).RunScenarioAsync(
                _ => Given_host_configuration_is_applied(),
                _ => _.Given_a_NaturalSchematic(),
                _ => _.Given_a_NaturalStateMachine(_.CurrentNaturalSchematic),
                _ => _.When_a_signal_is_sent(_.CurrentNaturalStateMachine, new ProvisioningSystem.ReserveSignal()),
                _ => _.Then_the_Machines_state_is_STATE(typeof(ProvisioningSystem.Provisioned)),
                _ => _.Then_no_exception_was_thrown());
        }
    }

    public class NaturalMachineTransitions
        : NaturalMachineTransitionScenarios<REstateNaturalContext>
    {
    }
}