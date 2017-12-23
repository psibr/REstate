using System;
using System.Collections.Generic;
using System.Text;
using REstate.Engine;
using Xunit;

namespace REstate.Tests.Units
{
    public class SchematicTests
    {
        [Fact]
        public void Schematic_has_correct_ToString_functionality()
        {
            // Arrange
            var schematic = REstateHost.Agent
                .CreateSchematic<string, string>("Schematic")
                .WithState("Initial", state => state
                    .AsInitialState())
                .Build();

            // Act
            var toString = schematic.ToString();

            // Assert
            Assert.NotNull(toString);
            Assert.Equal(DotGraphCartographer<string, string>.Instance.WriteMap(schematic.States), toString);
        }
    }
}
