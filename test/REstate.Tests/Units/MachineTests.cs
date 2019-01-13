using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.IoC;
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
            Assert.Equal(machine.MachineId, machine.ToString());
        }

        [Fact]
        public async Task Action_with_tail_returns_final_state()
        {
            // Arrange
            var host = new REstateHost();

            host.Agent().Configuration
                .RegisterConnector<ProcessAction>();

            var schematic = host.Agent()
                .CreateSchematic<string, string>(
                    schematicName: nameof(Action_with_tail_returns_final_state))
                .WithState("Idle", idle => idle
                    .AsInitialState())
                .WithState("Processing", processing => processing
                    .WithTransitionFrom("Idle", "Process")
                    .WithAction<ProcessAction>())
                .WithState("StillProcessing", stillProcessing => stillProcessing
                    .WithTransitionFrom("Processing", "Continue"))
                .WithState("Complete", complete => complete
                    .WithTransitionFrom("StillProcessing", "Complete"));

            var engine = host.Agent().GetStateEngine<string, string>();
            var machine = await engine.CreateMachineAsync(schematic);

            // Act
            var status = await machine.SendAsync("Process");

            // Assert
            Assert.Equal("Complete", status.State);
            Assert.Equal(3, status.CommitNumber);
        }

        /// <summary>
        /// An action that tails 2 actions before returning.
        /// </summary>
        public class ProcessAction
            : IAction<string, string>
        {
            public async Task InvokeAsync<TPayload>(
                ISchematic<string, string> schematic,
                IStateMachine<string, string> machine,
                Status<string> status,
                InputParameters<string, TPayload> inputParameters,
                IReadOnlyDictionary<string, string> connectorSettings,
                CancellationToken cancellationToken = default)
            {
                // Processing

                // Move next
                await Task.WhenAll(
                    machine.SendAsync("Continue", cancellationToken),
                    machine.SendAsync("Complete", cancellationToken));
            }
        }
    }
}
