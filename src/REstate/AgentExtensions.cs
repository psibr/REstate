using REstate.Schematics.Builder;
using REstate.Schematics.Builder.Implementation;

namespace REstate
{
    public static class AgentExtensions
    {
        public static ISchematicBuilder<TState, TInput> CreateSchematic<TState, TInput>(
            this IAgent agent,
            string schematicName) => new SchematicBuilder<TState, TInput>(schematicName);

        public static ILocalHost AsLocal(this IAgent agent) =>
            new LocalHost(agent);
    }
}