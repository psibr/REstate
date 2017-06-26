using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Engine
{
    public interface IStateEngine
    {
        Task<IStateMachine> CreateMachine(Schematic schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken);
        Task<string> CreateMachine(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken);
        Task DeleteMachine(string machineId, CancellationToken cancellationToken);
        Task<IStateMachine> GetMachine(string machineId, CancellationToken cancellationToken);
        Task<Machine> GetMachineInfoAsync(string machineId, CancellationToken cancellationToken);
        Task<IDictionary<string, string>> GetMachineMetadata(string machineId, CancellationToken cancellationToken);
        Task<Schematic> GetSchematic(string schematicName, CancellationToken cancellationToken);
        Task<string> GetSchematicDiagram(string schematicName, CancellationToken cancellationToken);
        Task<IEnumerable<Schematic>> ListSchematics(CancellationToken cancellationToken);
        string PreviewDiagram(Schematic schematic);
        Task<Schematic> StoreSchematic(Schematic schematic, CancellationToken cancellationToken);
    }
}