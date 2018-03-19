using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Connectors
{
    public interface IPrecondition
        : IConnector
    {
        
    }

    public interface IPrecondition<TState, TInput>
        : IPrecondition
    {
        Task<bool> ValidateAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default);
    }
}
