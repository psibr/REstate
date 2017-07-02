using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using REstate.Configuration.Builder;
using REstate.Configuration.Builder.Implementation;
using REstate.Engine;
using REstate.Engine.Repositories.InMemory;
using REstate.Engine.Services;
using REstate.IoC;
using REstate.IoC.BoDi;

namespace REstate
{
    public class REstateHost
    {
        static REstateHost()
        {
            Register(new BoDiComponentContainer(new ObjectContainer()));
        }

        private static IComponentContainer _container;

        public static IStateEngine<TState> GetStateEngine<TState>() =>
            _container.Resolve<IStateEngine<TState>>();

        /// <summary>
        /// Register's defaults and the REstate engine in a given container.
        /// </summary>
        /// <param name="container">An adapter to an IoC/DI container.</param>
        public static void Register(IComponentContainer container)
        {
            container.Register(typeof(IConnectorResolver<>), typeof(DefaultConnectorResolver<>));

            container.Register(typeof(IStateMachineFactory<>), typeof(REstateMachineFactory<>));
            container.Register(typeof(IStateEngine<>), typeof(StateEngine<>));

            container.Register(typeof(ICartographer<>), typeof(DotGraphCartographer<>));

            container.RegisterComponent(new InMemoryRepositoryComponent());

            _container = container;

            RegisterConnector("Console", typeof(ConsoleConnector<>));

            
        }

        public static void RegisterComponent(IComponent component) =>
            _container.RegisterComponent(component);

        public static void RegisterConnector(string connectorKey, Type connectorType) =>
            _container.Register(typeof(IConnector<>), connectorType, connectorKey);

        public static ISchematicBuilder<TState> CreateSchematic<TState>(string schematicName) =>
            new SchematicBuilder<TState>(schematicName);
    }
}
