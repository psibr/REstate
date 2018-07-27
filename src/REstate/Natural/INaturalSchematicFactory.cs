namespace REstate.Natural
{
    public interface INaturalSchematicFactory
    {
        INaturalSchematic BuildSchematic(IAgent agent);
    }
}