using System;
using System.Collections.Generic;
using System.Text;
using REstate.Schematics;
using Xunit;

namespace REstate.Tests.Units
{
    public class MachineTests
    {
        [Fact]
        public void Machine_has_correct_ToString_functionality()
        {
            // Arrange
            var schematic = REstateHost.Agent
                .CreateSchematic<string, string>("TestSchematic")
                .WithState("Initial", state => state
                    .AsInitialState())
                .Build();

            // Act
            var machine = REstateHost.Agent
                .GetStateEngine<string, string>()
                .CreateMachineAsync(schematic).GetAwaiter().GetResult();

            // Assert
            Assert.StartsWith($"{schematic.SchematicName}/", machine.ToString(), StringComparison.Ordinal);
            Assert.EndsWith(machine.MachineId, machine.ToString(), StringComparison.Ordinal);
        }
    }
}
