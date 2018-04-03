using System;

namespace REstate.Engine
{
    /// <summary>
    /// Indicates a request for a machine was made with an existing machineId
    /// </summary>
    public class MachineAlreadyExistException
        : Exception
    {
        public string RequestedMachineId { get; }

        public MachineAlreadyExistException(string machineId)
            : this(machineId, $"A Machine already exists with MachineId matching {machineId}.")
        {
        }

        public MachineAlreadyExistException(string machineId, string message)
            : base(message)
        {
            RequestedMachineId = machineId;
        }

        public MachineAlreadyExistException(string machineId, string message, Exception innerException)
            : base(message, innerException)
        {
            RequestedMachineId = machineId;
        }
    }
}
