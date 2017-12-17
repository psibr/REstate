using System;
using System.Linq;
using System.Reflection;
using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Engine.Connectors.Resolution;
using REstate.Engine.EventListeners;

namespace REstate.IoC
{
    public static class RegistrarExtensions
    {
        /// <summary>
        /// Registers an <see cref="IEntryConnector{TState,TInput}"/> or <see cref="IGuardianConnector{TState,TInput}"/>.
        /// </summary>
        /// <param name="registrar"></param>
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
        public static IConnectorRegistration RegisterConnector(this IRegistrar registrar, Type connectorType, string registrationName = null)
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
                registrar.Register(typeof(IEntryConnector<,>), connectorType, registrationKey);

                registered = true;
            }

            if (interfaces.Any(type => type == typeof(IGuardianConnector<,>)))
            {
                registrar.Register(typeof(IGuardianConnector<,>), connectorType, registrationKey);

                registered = true;
            }

            if (!registered)
                throw new ArgumentException(
                    message: "Type must be either IEntryConnector<,> or IGuardianConnector<,> to be registered.",
                    paramName: nameof(connectorType));

            return new ConnectorRegistration(registrar, connectorType);
        }

        private class ConnectorRegistration
            : IConnectorRegistration
        {
            private readonly IRegistrar _registrar;

            public ConnectorRegistration(IRegistrar registrar, Type connectorType)
            {
                ConnectorType = connectorType;
                _registrar = registrar;
            }

            private Type ConnectorType { get; }

            public IConnectorRegistration WithConfiguration<TConfiguration>(TConfiguration configuration)
                where TConfiguration : class, IConnectorConfiguration
            {
                _registrar.Register(configuration, configuration.Identifier);
                _registrar.Register(new ConnectorTypeToIdentifierMapping(ConnectorType, configuration.Identifier), configuration.Identifier);

                return this;
            }
        }

        public static void RegisterEventListener(this IRegistrar registrar, IEventListener listener)
        {
            registrar.Register(listener, listener.GetType().AssemblyQualifiedName);
        }
    }
}