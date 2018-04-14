using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.IoC;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace REstate.Tests.Units
{
    public class ActionTests
    {
        [Fact]
        public async Task Action_Can_Depend_On_AgentAsync()
        {
            // Arrange
            var host = new REstateHost();

            host.Agent().Configuration
                .RegisterConnector<ProcessAction>();

            var schematic = host.Agent()
                .CreateSchematic<string, string>(
                    schematicName: nameof(Action_Can_Depend_On_AgentAsync))
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
            Assert.Equal("Processing", status.State);
        }

        /// <summary>
        /// An action that depends on Agent
        /// </summary>
        public class ProcessAction
            : IAction<string, string>
        {
            public ProcessAction(IAgent agent)
            {
                Agent = agent;
            }

            public IAgent Agent { get; }

            public Task InvokeAsync<TPayload>(
                ISchematic<string, string> schematic,
                IStateMachine<string, string> machine,
                Status<string> status,
                InputParameters<string, TPayload> inputParameters,
                IReadOnlyDictionary<string, string> connectorSettings,
                CancellationToken cancellationToken = default)
            {
                // Processing
                var engine = Agent.GetStateEngine<string, string>();

                return Task.CompletedTask;
            }
        }

    }

}
