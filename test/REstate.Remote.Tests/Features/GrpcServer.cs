using System.Threading.Tasks;
using LightBDD.Framework;
using LightBDD.Framework.Scenarios.Contextual;
using LightBDD.Framework.Scenarios.Extended;
using LightBDD.XUnit2;
using REstate.Remote.Tests.Features.Context;
using REstate.Tests.Features.Templates;
using Xunit;

namespace REstate.Remote.Tests.Features
{
    [FeatureDescription(@"
In order to support cloud scaling
As a developer
I want to connect using gRPC to a remote REstate server")]
    [ScenarioCategory("REstate gRPC Server")]
    [ScenarioCategory("Remote")]
    [ScenarioCategory("gRPC")]
    [Collection("gRPC")]
    public class GrpcServer
        : REstateFeature<REstateRemoteContext<string, string>>
    {
        [Scenario]
        public async Task REstate_gRPC_Server_Sets_BoundPorts_on_start()
        {
            await Runner.WithContext(Context).RunScenarioAsync(
                _ => _.Given_a_new_host(),
                _ => _.When_a_REstate_gRPC_Server_is_created_and_started(),
                _ => _.Then_REstate_gRPC_Server_has_bound_ports());
        }
    }
}
