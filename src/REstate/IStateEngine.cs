using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate
{
    public interface IStateEngine<TState>
    {
        Task<IStateMachine<TState>> CreateMachineAsync(Schematic<TState> schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken);
        Task<string> CreateMachineAsync(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken);
        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken);
        Task<IStateMachine<TState>> GetMachineAsync(string machineId, CancellationToken cancellationToken);
        Task<State<TState>> GetMachineInfoAsync(string machineId, CancellationToken cancellationToken);
        Task<IDictionary<string, string>> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken);
        Task<Schematic<TState>> GetSchematicAsync(string schematicName, CancellationToken cancellationToken);
        Task<string> GetSchematicDiagramAsync(string schematicName, CancellationToken cancellationToken);
        Task<IEnumerable<Schematic<TState>>> ListSchematicsAsync(CancellationToken cancellationToken);
        string PreviewDiagram(Schematic<TState> schematic);
        Task<Schematic<TState>> StoreSchematicAsync(Schematic<TState> schematic, CancellationToken cancellationToken);
    }
}