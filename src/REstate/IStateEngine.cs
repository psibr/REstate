using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate
{
    public interface IStateEngine
    {
        Task<IStateMachine> CreateMachineAsync(Schematic schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken);
        Task<string> CreateMachineAsync(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken);
        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken);
        Task<IStateMachine> GetMachineAsync(string machineId, CancellationToken cancellationToken);
        Task<MachineStatus> GetMachineInfoAsync(string machineId, CancellationToken cancellationToken);
        Task<IDictionary<string, string>> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken);
        Task<Schematic> GetSchematicAsync(string schematicName, CancellationToken cancellationToken);
        Task<string> GetSchematicDiagramAsync(string schematicName, CancellationToken cancellationToken);
        Task<IEnumerable<Schematic>> ListSchematicsAsync(CancellationToken cancellationToken);
        string PreviewDiagram(Schematic schematic);
        Task<Schematic> StoreSchematicAsync(Schematic schematic, CancellationToken cancellationToken);
    }
}