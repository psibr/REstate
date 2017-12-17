using System;
using REstate.Engine;
using REstate.IoC;
using REstate.IoC.BoDi;
using REstate.Logging;
using REstate.Schematics;

namespace REstate
{
    /// <summary>
    /// The entry-point to REstate.
    /// </summary>
    public static class REstateHost
    {
        private static readonly object ConfigurationSyncRoot = new object();

        private static ILog Logger => LogProvider.GetLogger(typeof(REstateHost));

        private static IAgent CreateAgent()
        {
            if (HostConfiguration == null)
                lock (ConfigurationSyncRoot)
                    if (HostConfiguration == null)
                        UseContainer(
                            new BoDiComponentContainer(
                                new ObjectContainer()));

            return new Agent(HostConfiguration);
        }

        private static Lazy<IAgent> _agentLazy = new Lazy<IAgent>(CreateAgent);

        public static IAgent Agent => _agentLazy.Value;

        private static HostConfiguration HostConfiguration { get; set; }

        /// <summary>
        /// Instructs REstate to use a custom container for all components. 
        /// Defaults will be registered into the specified container.
        /// </summary>
        /// <param name="container">The container to use.</param>
        public static void UseContainer(IComponentContainer container)
        {
            lock (ConfigurationSyncRoot)
            {
                if (HostConfiguration != null)
                    throw new InvalidOperationException(
                        "Configuration has already been initialized; " +
                        "cannot replace container at this point.");

                Logger.Debug("REstateHost configuration initializing...");

                var configuration = new HostConfiguration(container);

                configuration.RegisterDefaults();

                HostConfiguration = configuration;

                Logger.Debug("REstateHost configuration initialized");
            }
        }

        /// <summary>
        /// Clears configuration and the Agent singleton.
        /// </summary>
        internal static void ResetAgent()
        {
            HostConfiguration = null;
            _agentLazy = new Lazy<IAgent>(CreateAgent);
        }

        public static string WriteStateMap<TState, TInput>(this Schematic<TState, TInput> schematic) =>
            HostConfiguration.Container
                .Resolve<ICartographer<TState, TInput>>()
                .WriteMap(schematic.States);

    }
}
