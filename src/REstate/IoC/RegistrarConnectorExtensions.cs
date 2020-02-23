using System;
using System.Linq;
using System.Reflection;
using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Engine.Connectors.Resolution;

namespace REstate.IoC
{
    [Flags]
    public enum ConnectorRegistrationTarget
    {
        Action = 1,
        Precondition = 2,
        BulkAction = 4
    }

    /// <summary>
    /// Methods to simplify the complexity of registering <see cref="IConnector"/>s.
    /// </summary>
    public static class RegistrarConnectorExtensions
    {
        private const ConnectorRegistrationTarget AllTargets =
            ConnectorRegistrationTarget.Action | ConnectorRegistrationTarget.Precondition | ConnectorRegistrationTarget.BulkAction;

        /// <summary>
        /// Registers an <see cref="IAction{TState,TInput}"/> or <see cref="IPrecondition{TState,TInput}"/> 
        /// using the AssemblyQualifiedName of the <see cref="Type"/> as the Identifier and resolvable by the 
        /// same value in a <see cref="Schematics.ConnectorKey"/>.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector, to use unbound generics, see the overload accepting a <see cref="Type"/> parameter.
        /// </typeparam>
        /// <param name="registrar"></param>
        /// <param name="registrationName">
        /// The name to register the type as in the <see cref="IComponentContainer"/>.
        /// <para />
        /// Registration names should ALWAYS be unique, overriding is NOT supported for safety/clarity reasons.
        /// <para />
        /// The default is the assembly qualified name of the type.
        /// </param>
        /// <remarks>
        /// Connectors are available for use by a <see cref="IStateMachine{TState,TInput}"/> when a <see cref="Schematics.ConnectorKey"/> 
        /// in its <see cref="Schematics.ISchematic{TState,TInput}"/> matches a <see cref="IConnectorConfiguration"/>,
        /// that has been registered in a <see cref="IConnectorRegistration"/>'s <c>WithConfiguration</c> method.
        /// </remarks>
        public static IConnectorRegistration RegisterConnector<TConnector>(
            this IRegistrar registrar,
            string registrationName = null,
            ConnectorRegistrationTarget connectorRegistrationMode = AllTargets)
            where TConnector : IConnector
        {
            return registrar.RegisterConnector(typeof(TConnector), registrationName, connectorRegistrationMode);
        }

        /// <summary>
        /// Registers an <see cref="IAction{TState,TInput}"/> or <see cref="IPrecondition{TState,TInput}"/>.
        /// </summary>
        /// <typeparam name="TConnector">
        /// The type of the connector, to use unbound generics, see the overload accepting a <see cref="Type"/> parameter.
        /// </typeparam>
        /// <param name="registrar"></param>
        /// <param name="configuration">
        /// A configuration object that includes an Identifier 
        /// that can be resolved using a <see cref="Schematics.ConnectorKey"/> in a <see cref="Schematics.ISchematic{TState,TInput}"/>.
        /// </param>
        /// <param name="registrationName">
        /// The name to register the type as in the <see cref="IComponentContainer"/>.
        /// <para />
        /// Registration names should ALWAYS be unique, overriding is NOT supported for safety/clarity reasons.
        /// <para />
        /// The default is the assembly qualified name of the type.
        /// </param>
        /// <remarks>
        /// Connectors are available for use by a <see cref="IStateMachine{TState,TInput}"/> when a <see cref="Schematics.ConnectorKey"/> 
        /// in its <see cref="Schematics.ISchematic{TState,TInput}"/> matches a <see cref="IConnectorConfiguration"/>,
        /// that has been registered in a <see cref="IConnectorRegistration"/>'s <c>WithConfiguration</c> method.
        /// </remarks>
        public static void RegisterConnector<TConnector>(
            this IRegistrar registrar,
            IConnectorConfiguration configuration,
            string registrationName = null,
            ConnectorRegistrationTarget connectorRegistrationMode = AllTargets)
            where TConnector : IConnector
        {
            registrar.RegisterConnector<TConnector>(registrationName, connectorRegistrationMode).WithConfiguration(configuration);
        }

        /// <summary>
        /// Registers an <see cref="IAction{TState,TInput}"/> or <see cref="IPrecondition{TState,TInput}"/>.
        /// </summary>
        /// <param name="registrar"></param>
        /// <param name="connectorType">
        /// The unbound generic type of the connector aka generic type definition. e.g. 
        /// <c>typeof(LoggingAction&lt;,&gt;)</c>
        /// </param>
        /// <param name="registrationName">
        /// The name to register the type as in the <see cref="IComponentContainer"/>.
        /// <para />
        /// Registration names should ALWAYS be unique, overriding is NOT supported for safety/clarity reasons.
        /// <para />
        /// The default is the assembly qualified name of the type (no version info).
        /// </param>
        /// <returns>A <see cref="IConnectorRegistration"/> that allows the caller to 
        /// register an <see cref="IConnectorConfiguration"/> that includes an Identifier 
        /// that can be resolved using a <see cref="Schematics.ConnectorKey"/> in a <see cref="Schematics.ISchematic{TState,TInput}"/>.
        /// </returns>
        /// <remarks>
        /// Connectors are available for use by a <see cref="IStateMachine{TState,TInput}"/> when a <see cref="Schematics.ConnectorKey"/> 
        /// in its <see cref="Schematics.ISchematic{TState,TInput}"/> matches a <see cref="IConnectorConfiguration"/>,
        /// that has been registered in a <see cref="IConnectorRegistration"/>'s <c>WithConfiguration</c> method.
        /// </remarks>
        public static IConnectorRegistration RegisterConnector(
            this IRegistrar registrar,
            Type connectorType,
            string registrationName = null,
            ConnectorRegistrationTarget connectorRegistrationMode = AllTargets)
        {
            var registrationKey = registrationName ?? TypeState.FromType(connectorType).GetConnectorKey();

            var interfaces = connectorType
                .GetInterfaces()
                .Where(type => type.IsConstructedGenericType)
                .Select(type => type.GetGenericTypeDefinition())
                .ToList();

            var registered = false;

            if (connectorRegistrationMode.HasFlag(ConnectorRegistrationTarget.Action) 
                && interfaces.Any(type => type == typeof(IAction<,>)))
            {
                registrar.Register(typeof(IAction<,>), connectorType, registrationKey);

                registered = true;
            }

            if (connectorRegistrationMode.HasFlag(ConnectorRegistrationTarget.Precondition) 
                && interfaces.Any(type => type == typeof(IPrecondition<,>)))
            {
                registrar.Register(typeof(IPrecondition<,>), connectorType, registrationKey);

                registered = true;
            }

            if (connectorRegistrationMode.HasFlag(ConnectorRegistrationTarget.BulkAction)
                && interfaces.Any(type => type == typeof(IBulkAction<,>)))
            {
                registrar.Register(typeof(IBulkAction<,>), connectorType, registrationKey);

                registered = true;
            }

            if (!registered)
                throw new ArgumentException(
                    message: "Type must be either IAction<,> or IPrecondition<,> to be registered.",
                    paramName: nameof(connectorType));

            return new ConnectorRegistration(registrar, connectorType);
        }

        /// <inheritdoc cref="IConnectorRegistration"/>
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
                // Register the configuration object, using its identifier as the key.
                _registrar.Register(configuration, configuration.Identifier);

                // Create a mapping (used by resolvers to choose which connector to load), 
                // and register it under the same Identifer.
                _registrar.Register(new ConnectorTypeToIdentifierMapping(ConnectorType, configuration.Identifier),
                    configuration.Identifier);

                return this;
            }
        }
    }
}
