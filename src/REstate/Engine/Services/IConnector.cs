using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Services
{
    public interface IConnector<TState, TInput>
        : IConnector
    {
        Task OnEntryAsync<TPayload>(
            IStateMachine<TState, TInput> machine,
            State<TState> state,
            TInput input,
            TPayload payload,
            IDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken);

        Task<bool> GuardAsync<TPayload>(
            IStateMachine<TState, TInput> machine,
            State<TState> state,
            TInput input,
            TPayload payload,
            IDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken);
    }

    public interface IConnector
    {
        string ConnectorKey { get; }
    }
}
