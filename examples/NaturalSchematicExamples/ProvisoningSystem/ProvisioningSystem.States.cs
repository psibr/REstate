using System.Threading;
using System.Threading.Tasks;
using REstate;
using REstate.Natural;

namespace NaturalSchematicExamples
{
    public partial class ProvisioningSystem
    {
        [Description("The resource has not yet been provisioned")]
        public class Unprovisioned
            : StateDefinition
        {
        }

        public class Provisioning
            // TSignal could be object to catch multiple signals
            : StateDefinition
            , IAcceptSignal<ReserveSignal>
        {
            [Description("Provision necessary resource and forwards the Reservation")]
            public async Task InvokeAsync(
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

            public Task<bool> ValidateAsync(
                ConnectorContext context,
                ReserveSignal signal,
                CancellationToken cancellationToken = default)
            {
                // validate the signal somehow. (Type checks are performed by base class)

                return Task.FromResult(true);
            }
        }

        public class Provisioned
            : StateDefinition
            , IAcceptSignal<ProvisioningCompleteSignal>
            , IAcceptSignal<ReserveSignal>
            , IAcceptSignal<ReleaseSignal>
        {
            [Description("Handle Reservation as a counter")]
            public Task InvokeAsync(
                ConnectorContext context,
                ReserveSignal provisionedSignal,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            [Description("Handle Release as a counter")]
            public Task InvokeAsync(
                ConnectorContext context,
                ReleaseSignal provisionedSignal,
                CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        public class Deprovisioning
            : StateDefinition
            , IAcceptSignal<DeprovisionSignal>
        {
            public Deprovisioning(IAgent agent)
            {
                Agent = agent;
            }

            public IAgent Agent { get; }

            [Description("Deprovision all resources allocated")]
            public async Task InvokeAsync(
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
