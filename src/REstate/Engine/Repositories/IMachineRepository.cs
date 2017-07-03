using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace REstate.Engine.Repositories
{
    public interface IMachineRepository<TState, TInput>
    {
        Task CreateMachineAsync(string schematicName, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken);

        Task CreateMachineAsync(Schematic<TState, TInput> schematic, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken);

        Task CreateMachineAsync(string schematicName, string machineId, CancellationToken cancellationToken);

        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken);

        Task<State<TState>> GetMachineStateAsync(string machineId, CancellationToken cancellationToken);

        Task<State<TState>> SetMachineStateAsync(string machineId, TState state, TInput input, Guid? lastCommitTag, CancellationToken cancellationToken);

        Task<State<TState>> SetMachineStateAsync(string machineId, TState state, TInput input, string parameterData, Guid? lastCommitTag, CancellationToken cancellationToken);

        Task<IDictionary<string, string>> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken);
    }
}
