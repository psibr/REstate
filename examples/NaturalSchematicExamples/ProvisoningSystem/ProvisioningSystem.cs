using System;
using REstate.Natural;
using REstate.Natural.Schematics.Builders;

namespace NaturalSchematicExamples
{
    public partial class ProvisioningSystem 
        : INaturalSchematicFactory
    {
        private static INaturalSchematic _schematic;

        public INaturalSchematic BuildSchematic(INaturalSchematicBuilder builder)
        {
            return _schematic ??= builder
                       .StartsIn<Unprovisioned>()
                       .For<Unprovisioned>().On<ReserveSignal>().MoveTo<Provisioning>()
                       .For<Provisioning>().On<ProvisioningCompleteSignal>().MoveTo<Provisioned>()
                       .For<Provisioned>().On<ReserveSignal>().MoveTo<Provisioned>()
                       .For<Provisioned>().On<ReleaseSignal>().MoveTo<Provisioned>()
                       .For<Provisioned>().On<DeprovisionSignal>().When<NoReservationsRemain>().MoveTo<Deprovisioning>()
                       .BuildAs(nameof(ProvisioningSystem));
        }
    }
}
