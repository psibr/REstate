using System.ComponentModel;
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

        public static IStateEngine<TState, TInput> GetStateEngine<TState, TInput>(this IAgent agent) =>
            REstateHost.HostConfiguration.Container.Resolve<IStateEngine<TState, TInput>>();

        public static TStateEngine GetStateEngine<TState, TInput, TStateEngine>(this IAgent agent)
            where TStateEngine : class, IStateEngine<TState, TInput> =>
            REstateHost.HostConfiguration.Container.Resolve<TStateEngine>();
    }
}