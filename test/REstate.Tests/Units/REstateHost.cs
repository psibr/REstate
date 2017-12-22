using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace REstate.Tests.Units
{
    public class REstateHostTests
    {
        [Fact]
        public void REstateHost_Agent_Is_Accessible()
        {
            var agent = REstateHost.Agent;

            Assert.NotNull(agent);
        }

    }
}
