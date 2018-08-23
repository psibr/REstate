using REstate.Schematics.Builders;

namespace REstate
{
    public static class AgentExtensions
    {
        public static ISchematicBuilder<TState, TInput> CreateSchematic<TState, TInput>(
            this IAgent agent,
            string schematicName) => new SchematicBuilder<TState, TInput>(schematicName);

        public static ILocalAgent AsLocal(this IAgent agent) =>
            new LocalAgent(agent);
    }
}
