using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace REstate.Tests.Units
{
    public class REstateHostTests
    {
        [Fact]
        public void REstateHost_Agent_Is_Accessible()
        {
            // Arrange

            // Act
            var agent = REstateHost.Agent;

            // Assert
            Assert.NotNull(agent);
        }

        [Fact]
        public void REstateHost_Agent_Handles_Concurrency()
        {
            // Arrange
            var agents = new List<IAgent>();

            // Act
            Parallel.ForEach(Enumerable.Range(0, 5), i =>
            {
                agents.Add(REstateHost.Agent);
            });

            // Assert
            Assert.NotEmpty(agents);
            agents.ForEach(Assert.NotNull);
        }

    }
}
