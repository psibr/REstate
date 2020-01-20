using System.Threading;
using System.Threading.Tasks;
using REstate.Natural;

namespace NaturalSchematicExamples
{
    public partial class ProvisioningSystem
    {
        public class NoReservationsRemain
            : NaturalPrecondition<DeprovisionSignal>
        {
            public override Task<bool> ValidateAsync(
                ConnectorContext context,
                DeprovisionSignal signal,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(true);
            }
        }
    }
}
