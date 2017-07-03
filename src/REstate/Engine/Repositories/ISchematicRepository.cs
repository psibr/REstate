using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface ISchematicRepository<TState, TInput>
    {
        Task<IEnumerable<Schematic<TState, TInput>>> ListSchematicsAsync(CancellationToken cancellationToken);

        Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken);

        Task<Schematic<TState, TInput>> RetrieveSchematicForMachineAsync(string machineId, CancellationToken cancellationToken);

        Task<Schematic<TState, TInput>> CreateSchematicAsync(Schematic<TState, TInput> schematic, string forkedFrom, CancellationToken cancellationToken);

        Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken);
    }
}
