using REstate.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface ISchematicRepository
    {
        Task<IEnumerable<Schematic>> ListSchematicsAsync(CancellationToken cancellationToken);

        Task<Schematic> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken);

        Task<Schematic> RetrieveSchematicForMachineAsync(string machineId, CancellationToken cancellationToken);

        Task<Schematic> CreateSchematicAsync(Schematic schematic, string forkedFrom, CancellationToken cancellationToken);

        Task<Schematic> StoreSchematicAsync(Schematic schematic, CancellationToken cancellationToken);
    }
}
