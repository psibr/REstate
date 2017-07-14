using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate
{
    public interface IStateEngine<TState, TInput>
    {
        Task<IStateMachine<TState, TInput>> CreateMachineAsync(Schematic<TState, TInput> schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken);
        Task<IStateMachine<TState, TInput>> CreateMachineAsync(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken);
        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken);
        Task<IStateMachine<TState, TInput>> GetMachineAsync(string machineId, CancellationToken cancellationToken);
        Task<Schematic<TState, TInput>> GetSchematicAsync(string schematicName, CancellationToken cancellationToken);
        Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken);
    }

    public interface ILocalStateEngine<TState, TInput>
        : IStateEngine<TState, TInput>
    {
    }
}