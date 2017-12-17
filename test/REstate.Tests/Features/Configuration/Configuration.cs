using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features.Configuration
{
    [FeatureDescription(@"
In order to extend REstate
As a developer
I want to use the configuration")]
    [ScenarioCategory("Configuration")]
    public partial class Configuration
        : FeatureFixture
    {

        [Scenario]
        public void Configuration_is_accessible_with_default_container()
        {
            Runner.RunScenario(
                _ => Given_a_new_host(),
                _ => When_configuration_is_accessed(),
                _ => Then_configuration_is_not_null(),
                _ => Then_configuration_has_a_container());
        }

        [Scenario]
        public void Configuration_is_accessible_after_switching_to_a_custom_container()
        {
            Runner.RunScenario(
                _ => Given_a_new_host(),
                _ => Given_a_new_container_is_configured(),
                _ => When_configuration_is_accessed(),
                _ => Then_configuration_is_not_null(),
                _ => Then_configuration_has_a_container());
        }
    }
}
