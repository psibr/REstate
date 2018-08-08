using REstate.Natural.Schematics.Builders;

namespace REstate.Natural
{
    public interface INaturalSchematicFactory
    {
        INaturalSchematic BuildSchematic(INaturalSchematicBuilder builder);
    }
}