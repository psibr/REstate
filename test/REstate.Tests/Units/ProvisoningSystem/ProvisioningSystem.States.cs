using System.Threading;
using System.Threading.Tasks;
using REstate.Natural;

namespace REstate.Tests.Units
{
    public partial class ProvisioningSystem
    {
        public class Unprovisioned
            : StateDefinition
        {
        }

        public class Provisioning
            : StateDefinition<ReserveSignal>
        {
            public override async Task InvokeAsync(
                ConnectorContext context,
                ReserveSignal reserveSignal,
                CancellationToken cancellationToken = default)
            {
                await context.Machine.SignalAsync(
                    new ProvisioningCompleteSignal
                    {
                        Reservation = reserveSignal
                    },
                    cancellationToken);
            }
        }

        public class Provisioned
            : StateDefinition<IProvisionedSignal>
        {
            public override Task InvokeAsync(
                ConnectorContext context,
                IProvisionedSignal provisionedSignal,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        public class Deprovisioning
            : StateDefinition<DeprovisionSignal>
        {
            public Deprovisioning(IAgent agent)
            {
                Agent = agent;
            }

            public IAgent Agent { get; }

            public override async Task InvokeAsync(
                ConnectorContext context,
                DeprovisionSignal deprovisionSignal,
                CancellationToken cancellationToken = default)
            {
                await Agent
                    .GetStateEngine<TypeState, TypeState>()
                    .DeleteMachineAsync(context.Machine.MachineId, cancellationToken);
            }
        }
    }
}
