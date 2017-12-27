using System;

namespace REstate.Engine
{
    /// <summary>
    /// Indicates a request for a schematic was made with an unknown identifier.
    /// </summary>
    public class SchematicDoesNotExistException
        : Exception
    {
        public string RequestedSchematicName { get; }

        public SchematicDoesNotExistException(string schematicName)
            : this(schematicName, $"No Schematic with name matching {schematicName} was found.")
        {
        }

        public SchematicDoesNotExistException(string schematicName, string message) 
            : base(message)
        {
            RequestedSchematicName = schematicName;
        }

        public SchematicDoesNotExistException(string schematicName, string message, Exception innerException) 
            : base(message, innerException)
        {
            RequestedSchematicName = schematicName;
        }
    }
}
