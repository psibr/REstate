using System;
using REstate.Schematics;

namespace REstate.Engine.Connectors
{
    public class ConnectorConfigurationNotFoundException
        : Exception
    {
        public ConnectorConfigurationNotFoundException(string schematicName, string machineId, ConnectorKey connectorKey) 
            : base($"No connector configuration matched {connectorKey.Identifier} " +
                   $"when entering a state on machine {machineId} which is defined with schematic {schematicName}.")
        {
            SchematicName = schematicName;
            MachineId = machineId;
            ConnectorKey = connectorKey;
        }

        public ConnectorConfigurationNotFoundException()
        {
        }

        public ConnectorConfigurationNotFoundException(string message) : base(message)
        {
        }

        public ConnectorConfigurationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string SchematicName { get; }

        public string MachineId { get; }

        public ConnectorKey ConnectorKey { get; }
    }

    public class ConnectorConfigurationNotFoundException<TState, TInput>
        : ConnectorConfigurationNotFoundException
    {

        public ConnectorConfigurationNotFoundException(ISchematic<TState, TInput> schematic, Status<TState> status, ConnectorKey connectorKey)
            : base(schematic.SchematicName, status.MachineId, connectorKey)
        {
            Schematic = schematic;
            Status = status;
        }

        public ConnectorConfigurationNotFoundException()
        {
        }

        public ConnectorConfigurationNotFoundException(string message) : base(message)
        {
        }

        public ConnectorConfigurationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ISchematic<TState, TInput> Schematic { get; }

        public Status<TState>? Status { get; }
    }
}
