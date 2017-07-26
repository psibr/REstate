using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using REstate.Schematics;

namespace REstate.Engine.Repositories
{
    public interface IMachineRepository<TState, TInput>
    {
        Task<MachineStatus<TState, TInput>> CreateMachineAsync(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken));

        Task<MachineStatus<TState, TInput>> CreateMachineAsync(Schematic<TState, TInput> schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken));

        Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken = default(CancellationToken));

        Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(string machineId, CancellationToken cancellationToken = default(CancellationToken));

        Task<MachineStatus<TState, TInput>> SetMachineStateAsync(string machineId, TState state, Guid? lastCommitTag, CancellationToken cancellationToken = default(CancellationToken));
    }
}
