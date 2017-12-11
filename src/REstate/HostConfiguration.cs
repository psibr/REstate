using System;
using System.Linq;
using System.Reflection;
using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Engine.Connectors.Console;
using REstate.Engine.Connectors.Logger;
using REstate.Engine.Connectors.Resolution;
using REstate.Engine.EventListeners;
using REstate.Engine.Repositories.InMemory;
using REstate.IoC;
using REstate.Logging;
using LogLevel = REstate.Engine.Connectors.Logger.LogLevel;

namespace REstate
{
    public interface IHostConfiguration
    {
        IConnectorRegistration RegisterConnector(Type connectorType, string name = null);

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

        /// <summary>
        /// Registers an <see cref="IEntryConnector{TState,TInput}"/> or <see cref="IGuardianConnector{TState,TInput}"/>.
        /// </summary>
        /// <param name="connectorType">
        /// The unbound generic type of the connector aka generic type definition. e.g. 
        /// <c>typeof(LoggerEntryConnector&lt;,&gt;)</c>
        /// </param>
        /// <param name="registrationName">
        /// The name to register the type as in the <see cref="IComponentContainer"/>.
        /// <para />
        /// Multiple registrations with the same name will override each other.
        /// <para />
        /// The default is the assembly qualified name of the type.
        /// </param>
        /// <returns>A <see cref="IConnectorRegistration"/> that allows the caller to 
        /// register an <see cref="IConnectorConfiguration"/> that includes an Identifier 
        /// that can be resolved using a <see cref="Schematics.ConnectorKey"/> in a <see cref="Schematics.ISchematic{TState,TInput}"/>.
        /// </returns>
        /// <remarks>
        /// Connectors are available for use by a <see cref="IStateMachine{TState,TInput}"/> when a <see cref="Schematics.ConnectorKey"/> 
        /// in it's <see cref="Schematics.ISchematic{TState,TInput}"/> matches a <see cref="IConnectorConfiguration"/>,
        /// that has been registered in a <see cref="IConnectorRegistration"/>'s <c>WithConfiguration</c> method.
        /// </remarks>
        public IConnectorRegistration RegisterConnector(Type connectorType, string registrationName = null)
        {
            var registrationKey = registrationName ?? connectorType.AssemblyQualifiedName;

            var interfaces = connectorType
                .GetInterfaces()
                .Where(type => type.IsConstructedGenericType)
                .Select(type => type.GetGenericTypeDefinition())
                .ToList();

            var registered = false;

            if (interfaces.Any(type => type == typeof(IEntryConnector<,>)))
            {
                Container.Register(typeof(IEntryConnector<,>), connectorType, registrationKey);

                registered = true;
            }

            if (interfaces.Any(type => type == typeof(IGuardianConnector<,>)))
            {
                Container.Register(typeof(IGuardianConnector<,>), connectorType, registrationKey);

                registered = true;
            }

            if(!registered)
                throw new ArgumentException(
                    message: "Type must be either IEntryConnector<,> or IGuardianConnector<,> to be registered.", 
                    paramName: nameof(connectorType));

            return new ConnectorRegistration(Container, connectorType);
        }

        private class ConnectorRegistration
            : IConnectorRegistration
        {
            private readonly IComponentContainer _container;

            public ConnectorRegistration(IComponentContainer container, Type connectorType)
            {
                ConnectorType = connectorType;
                _container = container;
            }

            private Type ConnectorType { get; }

            public IConnectorRegistration WithConfiguration<TConfiguration>(TConfiguration configuration)
                where TConfiguration : class, IConnectorConfiguration
            {
                _container.Register(configuration, configuration.Identifier);
                _container.Register(new ConnectorTypeToIdentifierMapping(ConnectorType, configuration.Identifier), configuration.Identifier);

                return this;
            }
        }

        public void RegisterEventListener(IEventListener listener)
        {
            Container.Register(listener, listener.GetType().AssemblyQualifiedName);
        }

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

            RegisterConnector(typeof(ConsoleEntryConnector<,>))
                .WithConfiguration(new ConsoleEntryConnectorConfiguration("console writeline"));

            RegisterConnector(typeof(ConsoleEntryConnector<,>))
                .WithConfiguration(new ConsoleEntryConnectorConfiguration("console write") { Mode = ConsoleWriteMode.Write });

            RegisterConnector(typeof(ConsoleGuardianConnector<,>))
                .WithConfiguration(new ConsoleGuardianConnectorConfiguration("console readline"));

            RegisterConnector(typeof(ConsoleGuardianConnector<,>))
                .WithConfiguration(new ConsoleGuardianConnectorConfiguration("console readkey") { Mode = ConsoleReadMode.ReadKey });

            RegisterConnector(typeof(LoggerEntryConnector<,>))
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log trace") { LogLevel = LogLevel.Trace })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log debug") { LogLevel = LogLevel.Debug })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log info") { LogLevel = LogLevel.Info })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log warn") { LogLevel = LogLevel.Warn })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log error") { LogLevel = LogLevel.Error })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log fatal") { LogLevel = LogLevel.Fatal });

            RegisterEventListener(LoggingEventListener.Trace);
        }
    }

}