using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Tests.Units
{
    public partial class ProvisioningSystem
    {
        public class Unprovisioned
            : StateDefinition
        {
        }

        public class Provisioning
            : StateDefinition<ReserveRequest>
        {
            public override async Task InvokeAsync(
                ISchematic<TypeState, TypeState> schematic,
                IStateMachine<TypeState, TypeState> machine,
                Status<TypeState> status,
                IInputParameters<TypeState, ReserveRequest> inputParameters,
                IReadOnlyDictionary<string, string> connectorSettings,
                CancellationToken cancellationToken = default)
            {
                await machine.SendAsync(typeof(ProvisioningCompleteRequest), new ProvisioningCompleteRequest { Reservation = inputParameters.Payload });
            }
        }

        public class Provisioned
            : StateDefinition<IProvisionedRequest>
        {
            public override Task InvokeAsync(
                ISchematic<TypeState, TypeState> schematic,
                IStateMachine<TypeState, TypeState> machine,
                Status<TypeState> status,
                IInputParameters<TypeState, IProvisionedRequest> inputParameters,
                IReadOnlyDictionary<string, string> connectorSettings,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        public class Deprovisioning
            : StateDefinition<DeprovisionRequest>
        {
            public Deprovisioning(IAgent agent)
            {
                Agent = agent;
            }

            public IAgent Agent { get; }

            public override async Task InvokeAsync(
                ISchematic<TypeState, TypeState> schematic,
                IStateMachine<TypeState, TypeState> machine,
                Status<TypeState> status,
                IInputParameters<TypeState, DeprovisionRequest> inputParameters,
                IReadOnlyDictionary<string, string> connectorSettings,
                CancellationToken cancellationToken = default)
            {
                await Agent.GetStateEngine<TypeState, TypeState>().DeleteMachineAsync(machine.MachineId);
            }
        }
    }
}
