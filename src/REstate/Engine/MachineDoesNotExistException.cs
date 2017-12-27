using System;

namespace REstate.Engine
{
    /// <summary>
    /// Indicates a request for a machine was made with an unknown or deleted machineId
    /// </summary>
    public class MachineDoesNotExistException
        : Exception
    {
        public string RequestedMachineId { get; }

        public MachineDoesNotExistException(string machineId)
            : this(machineId, $"No Machine with MachineId matching {machineId} was found.")
        {
        }

        public MachineDoesNotExistException(string machineId, string message) 
            : base(message)
        {
            RequestedMachineId = machineId;
        }

        public MachineDoesNotExistException(string machineId, string message, Exception innerException) 
            : base(message, innerException)
        {
            RequestedMachineId = machineId;
        }
    }
}
