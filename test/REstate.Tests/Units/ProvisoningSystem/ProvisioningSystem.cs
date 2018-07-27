using REstate.Natural;
using REstate.Natural.Schematics.Builders;

namespace REstate.Tests.Units
{
    public partial class ProvisioningSystem 
        : INaturalSchematicFactory
    {
        public INaturalSchematic BuildSchematic(IAgent agent) => 
            new NaturalSchematicBuilder(agent)
                .StartsIn<Unprovisioned>()
                .For<Unprovisioned>().On<ReserveSignal>().MoveTo<Provisioning>()
                .For<Provisioning>().On<ProvisioningCompleteSignal>().MoveTo<Provisioned>()
                .For<Provisioned>().On<ReserveSignal>().MoveTo<Provisioned>()
                .For<Provisioned>().On<ReleaseSignal>().MoveTo<Provisioned>()
                .For<Provisioned>().On<DeprovisionSignal>().When<NoReservationsRemain>().MoveTo<Deprovisioning>()
                .BuildAs(nameof(ProvisioningSystem));
    }
}
