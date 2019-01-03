using REstate.Natural;
using REstate.Natural.Schematics.Builders;

namespace REstate
{
    public static class NaturalFactoryAgentExtensions
    {
        public static INaturalStateEngine GetNaturalStateEngine(this IAgent agent)
        {
            return new NaturalStateEngine(agent, agent.GetStateEngine<TypeState, TypeState>());
        }

        public static INaturalSchematic ConstructSchematic<TNaturalSchematicFactory>(this IAgent agent)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            return new TNaturalSchematicFactory().BuildSchematic(new NaturalSchematicBuilder());
        }

        public static INaturalSchematic ConstructSchematic(this IAgent agent, INaturalSchematicFactory naturalSchematicFactory)
        {
            return naturalSchematicFactory.BuildSchematic(new NaturalSchematicBuilder());
        }
    }
}