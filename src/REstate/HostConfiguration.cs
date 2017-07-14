using System;
using REstate.Engine;
using REstate.Engine.Repositories.InMemory;
using REstate.Engine.Services;
using REstate.IoC;

namespace REstate
{
    public interface IHostConfiguration
    {
        void RegisterConnector(string connectorKey, Type connectorType);

        void RegisterComponent(IComponent component);

        /// <summary>
        /// Register's defaults and the REstateHost engine in a given container.
        /// </summary>
        /// <param name="container">An adapter to an IoC/DI container.</param>
        void Register(IComponentContainer container);
    }

    internal class HostConfiguration
        : IHostConfiguration
    {
        internal IComponentContainer Container;

        public HostConfiguration(IComponentContainer container)
        {
            Register(container);
        }

        public void RegisterConnector(string connectorKey, Type connectorType) =>
            Container.Register(typeof(IConnector<,>), connectorType, connectorKey);

        public void RegisterComponent(IComponent component) =>
            Container.RegisterComponent(component);

        /// <summary>
        /// Register's defaults and the REstateHost engine in a given container.
        /// </summary>
        /// <param name="container">An adapter to an IoC/DI container.</param>
        public void Register(IComponentContainer container)
        {
            container.Register(typeof(IConnectorResolver<,>), typeof(DefaultConnectorResolver<,>));

            container.Register(typeof(IStateMachineFactory<,>), typeof(REstateMachineFactory<,>));

            container.Register(typeof(IStateEngine<,>), typeof(StateEngine<,>));
            container.Register(typeof(ILocalStateEngine<,>), typeof(StateEngine<,>));

            container.Register(typeof(ICartographer<,>), typeof(DotGraphCartographer<,>));

            container.RegisterComponent(new InMemoryRepositoryComponent());

            Container = container;

            RegisterConnector("Console", typeof(ConsoleConnector<,>));
        }
    }
}