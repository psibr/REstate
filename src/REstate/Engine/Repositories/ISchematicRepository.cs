using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Repositories
{
    public interface ISchematicRepository<TState, TInput>
    {
        Task<IEnumerable<Schematic<TState, TInput>>> ListSchematicsAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken = default(CancellationToken));

        Task<Schematic<TState, TInput>> RetrieveSchematicForMachineAsync(string machineId, CancellationToken cancellationToken = default(CancellationToken));

        Task<Schematic<TState, TInput>> CreateSchematicAsync(Schematic<TState, TInput> schematic, string forkedFrom, CancellationToken cancellationToken = default(CancellationToken));

        Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default(CancellationToken));
    }
}
