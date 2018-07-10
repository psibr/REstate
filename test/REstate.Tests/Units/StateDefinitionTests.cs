using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using Xunit;

namespace REstate.Tests.Units
{
    public class StateDefinitionTests
    {
        [Fact]
        public async Task ProvisioningTest()
        {
            var host = new REstateHost();

            var schematic = host.Agent().CreateTypeSchematic<string>()
                .StartsIn<Unprovisioned>()
                .For<Unprovisioned>().On("Reserve").MoveTo<Provisioning>()
                .For<Provisioning>().On("Provisioned").MoveTo<Provisioned>()
                .For<Provisioned>().On("Release").When<IfSomeCase>().MoveTo<Provisioned>()
                .For<Provisioned>().On("Deprovision").MoveTo<Deprovisioning>()
                .For<Deprovisioning>().On("Deprovisioned").MoveTo<Deprovisioned>()
                .BuildAs("ProvisioningSystem");

            var machine = await host.Agent().GetStateEngine<TypeState, string>()
                .CreateMachineAsync(schematic);

            var newState = await machine.SendAsync("Reserve");
        }
    }

    public class Unprovisioned 
        : IStateDefinition
    {
    }

    public class Provisioning 
        : StateDefinition<string>
    {
        public override async Task InvokeAsync<TPayload>(
            ISchematic<TypeState, string> schematic,
            IStateMachine<TypeState, string> machine,
            Status<TypeState> status,
            InputParameters<string, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            await machine.SendAsync("Provisioned");
        }
    }

    public class Provisioned 
        : StateDefinition<string>
    {
        public override Task InvokeAsync<TPayload>(
            ISchematic<TypeState, string> schematic,
            IStateMachine<TypeState, string> machine,
            Status<TypeState> status,
            InputParameters<string, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    public class Deprovisioning
        : StateDefinition<string>
    {
        public override async Task InvokeAsync<TPayload>(
            ISchematic<TypeState, string> schematic,
            IStateMachine<TypeState, string> machine,
            Status<TypeState> status,
            InputParameters<string, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            await machine.SendAsync("Deprovisioned");
        }
    }

    public class Deprovisioned
        : StateDefinition<string>
    {
        public Deprovisioned(IAgent agent)
        {
            Agent = agent;
        }

        public IAgent Agent { get; }

        public override async Task InvokeAsync<TPayload>(
            ISchematic<TypeState, string> schematic,
            IStateMachine<TypeState, string> machine,
            Status<TypeState> status,
            InputParameters<string, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            await Agent.GetStateEngine<TypeState, string>().DeleteMachineAsync(machine.MachineId);
        }
    }

    public class IfSomeCase
        : IPrecondition<TypeState, string>
    {
        public Task<bool> ValidateAsync<TPayload>(
            ISchematic<TypeState, string> schematic,
            IStateMachine<TypeState, string> machine,
            Status<TypeState> status,
            InputParameters<string, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
