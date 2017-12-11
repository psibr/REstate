using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Connectors
{
    public interface IBulkEntryConnector
        : IConnector
    {
        
    }

    public interface IBulkEntryConnector<TState, TInput>
        : IBulkEntryConnector
    {
        IBulkEntryConnectorBatch<TState, TInput> CreateBatch();
    }

    public interface IBulkEntryConnectorBatch<TState, TInput>
    {
        Task OnBulkEntryAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);

        Task ExecuteBulkEntryAsync(CancellationToken cancellationToken = default);
    }
}