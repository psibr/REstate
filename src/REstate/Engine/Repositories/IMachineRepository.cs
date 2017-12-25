using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using REstate.Schematics;

namespace REstate.Engine.Repositories
{
    public interface IMachineRepository<TState, TInput>
    {
        Task<MachineStatus<TState, TInput>> CreateMachineAsync(string schematicName, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);

        Task<MachineStatus<TState, TInput>> CreateMachineAsync(Schematic<TState, TInput> schematic, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);

        Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken = default);

        Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(string machineId, CancellationToken cancellationToken = default);

        Task<MachineStatus<TState, TInput>> SetMachineStateAsync(string machineId, TState state, Guid? lastCommitTag, CancellationToken cancellationToken = default);
    }
}
