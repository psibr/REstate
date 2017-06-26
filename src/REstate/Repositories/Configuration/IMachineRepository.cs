using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace REstate.Engine.Repositories
{
    public interface IMachineRepository
    {
        Task CreateMachineAsync(string schematicName, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken);

        Task CreateMachineAsync(Schematic schematic, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken);

        Task CreateMachineAsync(string schematicName, string machineId, CancellationToken cancellationToken);

        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken);

        Task<Machine> GetMachineStateAsync(string machineId, CancellationToken cancellationToken);

        Task<Machine> SetMachineStateAsync(string machineId, string stateName, string triggerName, Guid? lastCommitTag, CancellationToken cancellationToken);

        Task<Machine> SetMachineStateAsync(string machineId, string stateName, string triggerName, string parameterData, Guid? lastCommitTag, CancellationToken cancellationToken);

        Task<IDictionary<string, string>> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken);
    }
}
