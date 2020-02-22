using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using Ninject;
using REstate.Tests.Features.Context;
using REstate.Tests.Features;

// ReSharper disable InconsistentNaming

namespace REstate.IoC.Ninject.Tests.Features
{
    [FeatureDescription(@"
In order to support IoC using my perferred DI container
As a developer
I want to transition states in machines.")]
    [ScenarioCategory("Machine Transition")]
    [ScenarioCategory("IoC")]
    [ScenarioCategory("Ninject")]
    public class MachineTransition
        : MachineTransitionScenarios<REstateContext<string, string>>
    {
        protected override Task<CompositeStep> Given_host_configuration_is_applied()
        {
            return Task.FromResult(
                CompositeStep
                    .DefineNew()
                    .WithContext(Context)
                    .AddAsyncSteps(
                        _ => _.Given_a_new_host_with_custom_ComponentContainer(new NinjectComponentContainer(new StandardKernel())))
                    .Build());
        }
    }
}
