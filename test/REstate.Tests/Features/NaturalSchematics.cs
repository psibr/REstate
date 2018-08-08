using System;
using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using NaturalSchematicExamples;
using REstate.Tests.Features.Context;

// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features
{
    [FeatureDescription(@"
In order to enable stored NaturalSchematic use-cases and wire-format use-cases
As a developer,
I want to be able to store and retrieve NaturalSchematics")]
    [ScenarioCategory("NaturalSchematics")]
    [ScenarioCategory("Schematics")]
    public abstract class NaturalSchematicsScenarios<TContext>
        : REstateFeature<TContext>
        where TContext : REstateContext<TypeState, TypeState>, new()
    {
        [Scenario]
        public async Task A_NaturalSchematic_can_be_stored_and_retrieved_with_no_exceptions()
        {
            var uniqueId = Guid.NewGuid().ToString();

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_NaturalSchematic(),
                _ => _.Given_a_NaturalSchematic_is_stored(Context.CurrentNaturalSchematic),
                _ => _.When_a_NaturalSchematic_is_retrieved(nameof(ProvisioningSystem)),
                _ => _.Then_no_exception_was_thrown());
        }
    }

    public class NaturalSchematics
        : NaturalSchematicsScenarios<REstateContext<TypeState, TypeState>>
    {
    }
}
