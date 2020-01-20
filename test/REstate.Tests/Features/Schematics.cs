using System;
using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using REstate.Schematics;
using REstate.Tests.Features.Context;

// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features
{
    [FeatureDescription(@"
In order to enable stored Schematic use-cases and wire-format use-cases
As a developer,
I want to be able to store and retrieve Schematics")]
    [ScenarioCategory("Schematics")]
    public abstract class SchematicsScenarios<TContext>
        : REstateFeature<TContext>
        where TContext : REstateContext<string, string>, new()
    {
        [Scenario]
        public async Task A_Schematic_with_an_action_that_was_stored_can_be_retrieved_with_a_valid_ConnectorKey()
        {
            var uniqueId = Guid.NewGuid().ToString();

            var schematicName = uniqueId;
            var machineId = uniqueId;
            var state = "initialState";
            ConnectorKey connectorKey = "log info";

            await Runner.WithContext(Context).RunScenarioAsync(
                _ =>   Given_host_configuration_is_applied(),
                _ => _.Given_a_Schematic_with_an_action_is_stored(uniqueId, state, connectorKey),
                _ => _.When_a_Schematic_is_retrieved(uniqueId),
                _ => _.Then_the_connector_key_should_have_a_valid_identifier(state, connectorKey));
        }
    }

    public class Schematics
        : SchematicsScenarios<REstateContext<string, string>>
    {
    }
}
