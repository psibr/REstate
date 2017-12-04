using System;
using REstate.Engine;
using REstate.IoC;
using REstate.IoC.BoDi;
using REstate.Logging;
using REstate.Schematics;

namespace REstate
{
    public static class REstateHost
    {
        private static readonly object ConfigurationSyncRoot = new object();

        private static ILog Logger => LogProvider.GetLogger(typeof(REstateHost));

        private static readonly Lazy<IAgent> AgentLazy = new Lazy<IAgent>(() =>
        {
            try
            {
                if (HostConfiguration == null)
                    UseContainer(new BoDiComponentContainer(new ObjectContainer()));
            }
            catch (InvalidOperationException)
            {
                // This is fine. What this means is a container was registered in-between null-check and lock acquisition.
            }

            return new Agent(HostConfiguration);
        });

        public static IAgent Agent => AgentLazy.Value;

        internal static HostConfiguration HostConfiguration { get; private set; }

        public static void UseContainer(IComponentContainer container)
        {
            lock (ConfigurationSyncRoot)
            {
                if(HostConfiguration != null)
                    throw new InvalidOperationException("Configuration has already been initialized; cannot replace container at this point.");

                Logger.Debug("REstateHost configuration initializing...");

                var configuration = new HostConfiguration(container);

                configuration.RegisterDefaults();

                HostConfiguration = configuration;

                Logger.Debug("REstateHost configuration initialized");
            }
        }


        public static string WriteStateMap<TState, TInput>(this Schematic<TState, TInput> schematic) =>
            HostConfiguration.Container
                .Resolve<ICartographer<TState, TInput>>()
                .WriteMap(schematic.States);

    }
}
