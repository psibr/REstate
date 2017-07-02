using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface ISchematicRepository<TState>
    {
        Task<IEnumerable<Schematic<TState>>> ListSchematicsAsync(CancellationToken cancellationToken);

        Task<Schematic<TState>> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken);

        Task<Schematic<TState>> RetrieveSchematicForMachineAsync(string machineId, CancellationToken cancellationToken);

        Task<Schematic<TState>> CreateSchematicAsync(Schematic<TState> schematic, string forkedFrom, CancellationToken cancellationToken);

        Task<Schematic<TState>> StoreSchematicAsync(Schematic<TState> schematic, CancellationToken cancellationToken);
    }
}
