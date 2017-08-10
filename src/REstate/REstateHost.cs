using System;
using System.ComponentModel;
using REstate.Engine;
using REstate.IoC.BoDi;
using REstate.Logging;
using REstate.Schematics;

namespace REstate
{
    public static class REstateHost
    {
        private static ILog Logger => LogProvider.GetLogger(typeof(REstateHost));

        private static readonly Lazy<IAgent> AgentLazy = new Lazy<IAgent>(() =>
        {
            Logger.Debug("REstateHost agent initializing...");

            return new Agent(HostConfiguration);
        });

        public static IAgent Agent => AgentLazy.Value;

        private static readonly HostConfiguration HostConfiguration =
            new HostConfiguration(
                new BoDiComponentContainer(
                    new ObjectContainer()));

        public static string WriteStateMap<TState, TInput>(this Schematic<TState, TInput> schematic) =>
            HostConfiguration.Container
                .Resolve<ICartographer<TState, TInput>>()
                .WriteMap(schematic.States);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static IStateEngine<TState, TInput> GetStateEngine<TState, TInput>(this IAgent agent) =>
            HostConfiguration.Container.Resolve<IStateEngine<TState, TInput>>();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static TStateEngine GetStateEngine<TState, TInput, TStateEngine>(this IAgent agent)
            where TStateEngine : class, IStateEngine<TState, TInput> =>
                HostConfiguration.Container.Resolve<TStateEngine>();
    }
}
