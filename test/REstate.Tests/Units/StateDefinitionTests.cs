using System.Threading.Tasks;
using Xunit;
using static REstate.Tests.Units.ProvisioningSystem;

namespace REstate.Tests.Units
{
    public class StateDefinitionTests
    {
        [Fact]
        public async Task ProvisioningTest()
        {
            var host = new REstateHost();

            var machine = await host.Agent().CreateNaturalMachineAsync<ProvisioningSystem>();

            var newState = await machine.SignalAsync(new ReserveSignal());
            newState = await machine.SignalAsync(new ReleaseSignal());
            newState = await machine.SignalAsync(new DeprovisionSignal());
        }
    }    
}
