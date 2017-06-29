using System;
using REstate.Configuration.Builder;
using REstate.Engine;
using REstate.Engine.Repositories;
using REstate.Engine.Repositories.InMemory;
using REstate.Engine.Services;
using REstate.IoC;
using REstate.IoC.TinyIoC;
using static REstate.Serialization.SimpleJson.SimpleJson;

namespace REstate
{
    public class REstateHost
    {
        static REstateHost()
        {
            Register(new TinyIoCContainerAdapter(TinyIoCContainer.Current));
        }

        private static IComponentContainer _container;

        public static IStateEngine Engine =>
            _container.Resolve<IStateEngine>();

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
                serializer: SerializeObject,
                deserializer: DeserializeObject));

            container.Register<ICartographer>(new DotGraphCartographer());

            container.RegisterComponent(new InMemoryRepositoryComponent());

            _container = container;

            RegisterConnector(new ConsoleWriterConnector());
        }

        public static void RegisterComponent(IComponent component) =>
            _container.RegisterComponent(component);

        public static void RegisterConnector(IConnectorFactory connectorFactory) =>
            _container.Register(connectorFactory, connectorFactory.ConnectorKey);

        public static void RegisterConnector(IConnector connector) =>
            _container.Register<IConnectorFactory>(
                instance: new SingletonConnectorFactory(connector),
                name: connector.ConnectorKey);

        public static ISchematicBuilder CreateSchematic(string schematicName) =>
            new SchematicBuilder(schematicName);
    }
}
