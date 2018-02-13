using System;
using REstate.Engine;
using REstate.Engine.Connectors.Logger;
using REstate.Engine.Connectors.Resolution;
using REstate.Engine.EventListeners;
using REstate.Engine.Repositories.InMemory;
using REstate.IoC;
using REstate.Logging;

namespace REstate
{
    public interface IHostConfiguration
    {
        void RegisterComponent(IComponent component);

        void RegisterComponent<TComponent>() where TComponent : IComponent, new();
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

        public void RegisterComponent(IComponent component) =>
            Container.RegisterComponent(component);

        public void RegisterComponent<TComponent>() 
            where TComponent : IComponent, new()
        {
            Container.RegisterComponent(new TComponent());
        }

        /// <summary>
        /// Registers defaults and the REstate engine.
        /// </summary>
        internal void RegisterDefaults()
        {
            Logger.DebugFormat("Registering default components into container of runtime type: {containerType}.", Container.GetType());

            Container.Register(typeof(IConnectorResolver<,>), typeof(DefaultConnectorResolver<,>));

            Container.Register(typeof(IStateMachineFactory<,>), typeof(REstateMachineFactory<,>));

            Container.Register(typeof(IStateEngine<,>), typeof(StateEngine<,>));
            Container.Register(typeof(ILocalStateEngine<,>), typeof(StateEngine<,>));

            Container.Register(typeof(ICartographer<,>), typeof(DotGraphCartographer<,>));

            Container.RegisterComponent(new InMemoryRepositoryComponent());

            Container.RegisterComponent(new LoggerEntryConnectorComponent());

            Container.RegisterEventListener(LoggingEventListener.Trace);
        }
    }

}