using Newtonsoft.Json;
using REstate.Engine;
using REstate.Engine.Repositories;
using REstate.Engine.Repositories.InMemory;
using REstate.Engine.Services;
using REstate.IoC;
using REstate.IoC.TinyIoC;
using REstate.Services;

namespace REstate
{ 
    public class REstateHost
    {
        static REstateHost()
        {
            Register(new TinyIoCContainerAdapter(TinyIoCContainer.Current));
        }

        private static IComponentContainer Container;

        public static IStateEngine Engine => Container.Resolve<IStateEngine>();

        /// <summary>
        /// Register's defaults and the REstate engine in a given container.
        /// </summary>
        /// <param name="container">An adapter to an IoC/DI container.</param>
        public static void Register(IComponentContainer container)
        {
            container.Register<IConnectorFactoryResolver>(c => 
                new DefaultConnectorFactoryResolver(
                    connectorFactories: c.ResolveAll<IConnectorFactory>()));

            container.Register<IStateMachineFactory>(c =>
                new REstateMachineFactory(
                    c.Resolve<IConnectorFactoryResolver>(),
                    c.Resolve<IRepositoryContextFactory>(),
                    c.Resolve<ICartographer>()));

            container.Register<IStateEngine>(c =>
                new StateEngine(
                    c.Resolve<IStateMachineFactory>(),
                    c.Resolve<IRepositoryContextFactory>(),
                    c.Resolve<StringSerializer>()));

            container.Register(new StringSerializer(
                serializer: (obj) => JsonConvert.SerializeObject(obj),
                deserializer: (str) => JsonConvert.DeserializeObject(str)));

            container.Register<ICartographer>(new DotGraphCartographer());

            container.RegisterComponent(new InMemoryRepositoryComponent());

            Container = container;

            RegisterConnector(new ConsoleWriterConnector());
        }

        public static void RegisterComponent(IComponent component)
        {
            Container.RegisterComponent(component);
        }

        public static void RegisterConnector(IConnectorFactory connectorFactory)
        {
            Container.Register(connectorFactory, connectorFactory.ConnectorKey);
        }

        public static void RegisterConnector(IConnector connector)
        {
            Container.Register<IConnectorFactory>(new SingletonConnectorFactory(connector), connector.ConnectorKey);
        }
    }
}
