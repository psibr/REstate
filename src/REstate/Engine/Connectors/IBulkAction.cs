using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Connectors
{
    public interface IBulkAction
        : IConnector
    {
        
    }

    public interface IBulkAction<TState, TInput>
        : IBulkAction
    {
        IBulkActionBatch<TState, TInput> CreateBatch();
    }

    public interface IBulkActionBatch<TState, TInput>
    {
        Task Stage<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);

        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
