using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
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

            var machine = await host.Agent().CreateMachineAsync<ProvisioningSystem>();

            var newState = await machine.SendAsync(typeof(ReserveRequest), new ReserveRequest());
            newState = await machine.SendAsync(typeof(ReleaseRequest), new ReleaseRequest());
            newState = await machine.SendAsync(typeof(DeprovisionRequest), new DeprovisionRequest());
        }
    }    
}
