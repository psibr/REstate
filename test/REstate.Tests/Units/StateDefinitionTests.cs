using System.Threading.Tasks;
using Xunit;
using NaturalSchematicExamples;

namespace REstate.Tests.Units
{
    using static ProvisioningSystem;

    public class StateDefinitionTests
    {
        [Fact]
        public async Task ProvisioningTest()
        {
            var host = new REstateHost();

            var machine = await host.Agent().CreateMachineAsync<ProvisioningSystem>();

            var newState = await machine.SignalAsync(new ReserveSignal());
            newState = await machine.SignalAsync(new ReleaseSignal());
            newState = await machine.SignalAsync(new DeprovisionSignal());
        }
    }    
}
