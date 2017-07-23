using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Services
{
    public interface IBulkEntryConnector<TState, TInput>
    {
        Task OnInitialEntryAsync(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken));

        Task ExecuteBulkEntryAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}