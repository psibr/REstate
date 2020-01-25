extern alias v930;
extern alias vCurrent;
using System;
using Xunit;

namespace REstate.Compatibility.Tests
{
    public class LoadingTests
    {
        [Fact]
        public void AllVersionsCanLoadAgent()
        {
            var v930Agent = v930::REstate.REstateHost.Agent;
            var vCurrentAgent = vCurrent::REstate.REstateHost.Agent;

            Assert.NotNull(v930Agent);
            Assert.NotNull(vCurrentAgent);
        }
    }
}
