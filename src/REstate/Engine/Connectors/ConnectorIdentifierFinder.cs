using System.Collections.Generic;
using System.Linq;
using REstate.Schematics;

namespace REstate.Engine.Connectors
{
    public static class ConnectorIdentifierFinder
    {
        public static ConnectorKey FindConnectorKey<TState, TInput>(this ISchematic<TState, TInput> schematic, TState state) => 
            schematic.States.Single(kvp => kvp.Key.Equals(state)).Value.Action.ConnectorKey;

        public static TConfiguration FindConfiguration<TConfiguration, TState, TInput>(
            this IEnumerable<TConfiguration> configurations, 
            ISchematic<TState, TInput> schematic, 
            Status<TState> status)
            where TConfiguration : IConnectorConfiguration
        {
            var connectorKey = schematic.FindConnectorKey(status.State);

            var configuration = configurations.SingleOrDefault(c => c.Identifier == connectorKey.Identifier);

            if (configuration == null)
                throw new ConnectorConfigurationNotFoundException<TState,TInput>(schematic, status, connectorKey);

            return configuration;
        }
    }
}
