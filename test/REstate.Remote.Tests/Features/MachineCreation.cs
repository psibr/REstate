using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using REstate.Remote.Tests.Features.Context;
using REstate.Tests.Features;

// ReSharper disable InconsistentNaming

namespace REstate.Remote.Tests.Features
{
    [FeatureDescription(@"
In order to support cloud scaling
As a developer
I want to create Machines from Schematics on a remote server")]
    [ScenarioCategory("Machine Creation")]
    [ScenarioCategory("Remote")]
    [ScenarioCategory("gRPC")]
    public class MachineCreation
        : MachineCreationScenarios<REstateRemoteContext<string, string>>
    {
        protected override Task<CompositeStep> Given_host_configuration_is_applied()
        {
            return Task.FromResult(
                CompositeStep
                    .DefineNew()
                    .WithContext(Context)
                    .AddAsyncSteps(
                        _ => _.Given_a_new_host(),
                        _ => _.Given_a_REstate_gRPC_Server_running(),
                        _ => _.Given_the_default_agent_is_gRPC_remote())
                    .Build());
        }
    }
}
