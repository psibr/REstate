using REstate.Configuration;
using REstate.Configuration.Builder;
using REstate.Configuration.Builder.Implementation;
using REstate.Engine;
using REstate.IoC.BoDi;

namespace REstate
{
    public static class REstateHost
    {
        public static IAgent Agent => new Agent(HostConfiguration);

        private static readonly HostConfiguration HostConfiguration =
            new HostConfiguration(
                new BoDiComponentContainer(
                    new ObjectContainer()));

        public static ILocalHost AsLocal(this IAgent agent) =>
            new LocalHost(agent);

        public static string WriteStateMap<TState, TInput>(this Schematic<TState, TInput> schematic) => 
            HostConfiguration.Container
                .Resolve<ICartographer<TState, TInput>>()
                .WriteMap(schematic.StateConfigurations);

        public static IStateEngine<TState, TInput> GetStateEngine<TState, TInput>(this IAgent agent) =>
            HostConfiguration.Container.Resolve<IStateEngine<TState, TInput>>();

        public static TStateEngine GetStateEngine<TState, TInput, TStateEngine>(this IAgent agent) 
            where TStateEngine : class, IStateEngine<TState, TInput> =>
                HostConfiguration.Container.Resolve<TStateEngine>();

        public static ISchematicBuilder<TState, TInput> CreateSchematic<TState, TInput>(this IAgent agent, string schematicName) =>
            new SchematicBuilder<TState, TInput>(schematicName);
    }
}
