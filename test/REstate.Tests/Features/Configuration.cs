using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using REstate.IoC.BoDi;
using REstate.Tests.Features.Context;

// ReSharper disable InconsistentNaming

namespace REstate.Tests.Features
{
    [FeatureDescription(@"
In order to extend REstate
As a developer
I want to use the configuration")]
    [ScenarioCategory("Configuration")]
    public class Configuration
        : FeatureFixture
    {
        [Scenario]
        public async Task Configuration_is_accessible_with_default_container()
        {
            await Runner.WithContext<REstateContext>().RunScenarioAsync(
                _ => _.Given_a_new_host(),
                _ => _.When_configuration_is_accessed(),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_configuration_is_not_null(),
                _ => _.Then_configuration_has_a_container());
        }

        [Scenario]
        public async Task Configuration_is_accessible_with_custom_container()
        {
            var customComponentContainer = new BoDiComponentContainer(new ObjectContainer());

            await Runner.WithContext<REstateContext>().RunScenarioAsync(
                _ => _.Given_a_new_host_with_custom_ComponentContainer(customComponentContainer),
                _ => _.When_configuration_is_accessed(),
                _ => _.Then_no_exception_was_thrown(),
                _ => _.Then_configuration_is_not_null(),
                _ => _.Then_configuration_has_a_container());
        }
    }
}
