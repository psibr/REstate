using REstate.Engine;
using REstate.Schematics.Builder;
using REstate.Schematics.Builder.Implementation;

namespace REstate
{
    public static class AgentExtensions
    {
        public static ISchematicBuilder<TState, TInput> CreateSchematic<TState, TInput>(
            this IAgent agent,
            string schematicName) => new SchematicBuilder<TState, TInput>(schematicName);

        public static ILocalAgent AsLocal(this IAgent agent) =>
            new LocalAgent(agent);

        public static IStateEngine<TState, TInput> GetStateEngine<TState, TInput>(this IAgent agent) =>
            ((HostConfiguration)REstateHost.Agent.Configuration).Container.Resolve<IStateEngine<TState, TInput>>();

        public static TStateEngine GetStateEngine<TState, TInput, TStateEngine>(this IAgent agent)
            where TStateEngine : class, IStateEngine<TState, TInput> =>
            ((HostConfiguration)REstateHost.Agent.Configuration).Container.Resolve<TStateEngine>();
    }
}