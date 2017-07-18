using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate
{
    public interface IStateMachine<TState, TInput>
    {
        Task<ISchematic<TState, TInput>> GetSchematicAsync(
            CancellationToken cancellationToken = default(CancellationToken));

        string MachineId { get; }

        Task<State<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            CancellationToken cancellationToken = default(CancellationToken));

        Task<State<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            Guid lastCommitTag,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<State<TState>> SendAsync(
            TInput input,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<State<TState>> SendAsync(
            TInput input,
            Guid lastCommitTag,
            CancellationToken cancellationToken = default(CancellationToken));

        Task<bool> IsInStateAsync(State<TState> state, CancellationToken cancellationToken = default(CancellationToken));

        Task<State<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<ICollection<TInput>> GetPermittedInputAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
