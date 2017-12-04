using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate
{
    public interface IStateEngine<TState, TInput>
    {
        Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default);

        Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            ISchematic<TState, TInput> schematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default);

        Task<IStateMachine<TState, TInput>> CreateMachineAsync(
            string schematicName,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default);

        Task BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        Task BulkCreateMachinesAsync(
            ISchematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        Task BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default);

        Task<IStateMachine<TState, TInput>> GetMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default);

        Task<ISchematic<TState, TInput>> GetSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default);

        Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default);

        Task<ISchematic<TState, TInput>> StoreSchematicAsync(
            ISchematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default);
    }
}