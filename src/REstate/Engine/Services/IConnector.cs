using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Services
{
    public interface IConnector<TState, TInput>
        : IConnector
    {
        Task OnEntryAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);

        Task<bool> GuardAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);

        IBulkEntryConnector<TState, TInput> GetBulkEntryConnector();
    }

    public interface IConnector
    {
        ConnectorKey Key { get; }
    }
}
