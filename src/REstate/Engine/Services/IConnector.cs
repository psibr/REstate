using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Services
{
    public interface IConnector<TState>
        : IConnector
    {

        Func<CancellationToken, Task> ConstructAction(IStateMachine<TState> machineInstance, State<TState> state, string payload, IDictionary<string, string> connectorSettings);

        Func<State<TState>, Input, string, CancellationToken, Task<bool>> ConstructPredicate(IStateMachine<TState> machineInstance, IDictionary<string, string> connectorSettings);
    }

    public interface IConnector
    {
        string ConnectorKey { get; }
    }
}
