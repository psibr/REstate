using System;
using System.Linq.Expressions;
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
    public class REstateHost
        : IHasAgentLazy
    {
        public REstateHost(IComponentContainer container)
            : this()
        {
            var configuration = new HostConfiguration(container);

            configuration.RegisterDefaults();

            HostConfiguration = configuration;
        }

        public REstateHost()
        {
            ((IHasAgentLazy)this).AgentLazy = new Lazy<IAgent>(CreateAgent);
        }

        internal static readonly REstateHost SharedInstance = new REstateHost();

        internal readonly object ConfigurationSyncRoot = new object();

        private IAgent CreateAgent()
        {
            TryUseContainer(this, new BoDiComponentContainer(new ObjectContainer()));

            return new Agent(HostConfiguration);

        }

        Lazy<IAgent> IHasAgentLazy.AgentLazy { get; set; }

        public static IAgent Agent => SharedInstance.Agent();

        private HostConfiguration HostConfiguration { get; set; }

        /// <summary>
        /// Instructs REstate to use a custom container for all components. 
        /// Defaults will be registered into the specified container.
        /// </summary>
        /// <param name="container">The container to use.</param>
        public static void UseContainer(IComponentContainer container)
        {
            if (!TryUseContainer(SharedInstance, container))
                throw new InvalidOperationException(
                    "Configuration has already been initialized; " +
                    "cannot replace container at this point.");
        }

        /// <summary>
        /// Instructs REstate to use a custom container for all components. 
        /// Defaults will be registered into the specified container.
        /// </summary>
        /// <param name="host">The host to modify</param>
        /// <param name="container">The container to use.</param>
        private static bool TryUseContainer(REstateHost host, IComponentContainer container)
        {
            if (host.HostConfiguration == null)
            {
                lock (host.ConfigurationSyncRoot)
                {
                    if (host.HostConfiguration == null)
                    {
                        var configuration = new HostConfiguration(container);

                        configuration.RegisterDefaults();

                        host.HostConfiguration = configuration;

                        return true;
                    }
                }
            }

            return false;
        }
    }

    internal interface IHasAgentLazy
    {
        Lazy<IAgent> AgentLazy { get; set; }
    }

    public static class REstateHostExtensions
    {
        public static IAgent Agent(this REstateHost host)
        {
            return ((IHasAgentLazy)host).AgentLazy.Value;
        }
    }
}
