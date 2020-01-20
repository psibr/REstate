using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using Ninject;
using REstate.IoC.Ninject;
using REstate.Tests.Features.Context;
using REstate.Tests.Features;

// ReSharper disable InconsistentNaming

namespace REstate.IoC.Ninject.Tests.Features
{
    [FeatureDescription(@"
In order to support IoC using my preferred DI container
As a developer
I want to store and retrieve NaturalSchematics.")]
    [ScenarioCategory("NaturalSchematics")]
    [ScenarioCategory("Schematics")]
    [ScenarioCategory("IoC")]
    [ScenarioCategory("Ninject")]
    public class NaturalSchematics
        : NaturalSchematicsScenarios<REstateContext<TypeState, TypeState>>
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


    [FeatureDescription(@"
In order to support IoC using my preferred DI container
As a developer
I want to be able to activate transitions on machines using the Natural syntax.")]
    [ScenarioCategory("NaturalMachineTransitions")]
    [ScenarioCategory("Machines")]
    [ScenarioCategory("IoC")]
    [ScenarioCategory("Ninject")]
    public class NaturalMachineTransitions
        : NaturalMachineTransitionScenarios<REstateNaturalContext>
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
