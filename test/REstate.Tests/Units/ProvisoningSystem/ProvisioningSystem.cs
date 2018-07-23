using REstate.Schematics;
using REstate.Schematics.Builders;

namespace REstate.Tests.Units
{
    public partial class ProvisioningSystem : ITypeSchematicFactory
    {
        public Schematic<TypeState, TypeState> BuildSchematic(IAgent agent) => 
            new TypeSchematicBuilder(agent)
                .StartsIn<Unprovisioned>()
                .For<Unprovisioned>().On<ReserveRequest>().MoveTo<Provisioning>()
                .For<Provisioning>().On<ProvisioningCompleteRequest>().MoveTo<Provisioned>()
                .For<Provisioned>().On<ReserveRequest>().MoveTo<Provisioned>()
                .For<Provisioned>().On<ReleaseRequest>().When<SomethingIsValid>().MoveTo<Provisioned>()
                .For<Provisioned>().On<DeprovisionRequest>().MoveTo<Deprovisioning>()
                .BuildAs(nameof(ProvisioningSystem));
    }
}
