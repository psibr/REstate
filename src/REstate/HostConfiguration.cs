using System;
using REstate.Engine;
using REstate.Engine.Repositories.InMemory;
using REstate.Engine.Services;
using REstate.Engine.Services.ConnectorResolvers;
using REstate.IoC;
using REstate.Logging;

namespace REstate
{
    public interface IHostConfiguration
    {
        void RegisterConnector(ConnectorKey key, Type connectorType);

        void RegisterComponent(IComponent component);

        /// <summary>
        /// Register's defaults and the REstateHost engine.
        /// </summary>
        void RegisterDefaults();
    }

    internal class HostConfiguration
        : IHostConfiguration
    {
        private static ILog Logger => LogProvider.For<HostConfiguration>();

        internal IComponentContainer Container { get; }

        public HostConfiguration(IComponentContainer container)
        {
            Container = container;
        }

        public void RegisterConnector(ConnectorKey key, Type connectorType) =>
            Container.Register(typeof(IConnector<,>), connectorType, key.Name); // TODO: Include version in registration name

        public void RegisterComponent(IComponent component) =>
            Container.RegisterComponent(component);

        /// <summary>
        /// Registers defaults and the REstateHost engine.
        /// </summary>
        public void RegisterDefaults()
        {
            Logger.DebugFormat("Registering default components into container of runtime type: {containerType}.", Container.GetType());

            Container.Register(typeof(IConnectorResolver<,>), typeof(DefaultConnectorResolver<,>));

            Container.Register(typeof(IStateMachineFactory<,>), typeof(REstateMachineFactory<,>));

            Container.Register(typeof(IStateEngine<,>), typeof(StateEngine<,>));
            Container.Register(typeof(ILocalStateEngine<,>), typeof(StateEngine<,>));

            Container.Register(typeof(ICartographer<,>), typeof(DotGraphCartographer<,>));

            Container.RegisterComponent(new InMemoryRepositoryComponent());

            RegisterConnector(ConsoleConnector<object, object>.Key, typeof(ConsoleConnector<,>));
            RegisterConnector(LogConnector<object, object>.Key, typeof(LogConnector<,>));

            Container.Register<IEventListener>(new TraceEventListener(), nameof(TraceEventListener));
        }
    }
}