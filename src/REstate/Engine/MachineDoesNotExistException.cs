using System;

namespace REstate.Engine
{
    /// <summary>
    /// Indicates a request for a machine was made with an unknown or deleted machineId
    /// </summary>
    public class MachineDoesNotExistException
        : Exception
    {
        public MachineDoesNotExistException()
        {
        }

        public MachineDoesNotExistException(string message) 
            : base(message)
        {
        }

        public MachineDoesNotExistException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
