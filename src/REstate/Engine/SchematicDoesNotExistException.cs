using System;

namespace REstate.Engine
{
    /// <summary>
    /// Indicates a request for a schematic was made with an unknown name.
    /// </summary>
    public class SchematicDoesNotExistException
        : Exception
    {
        public SchematicDoesNotExistException()
        {
        }

        public SchematicDoesNotExistException(string message) 
            : base(message)
        {
        }

        public SchematicDoesNotExistException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
