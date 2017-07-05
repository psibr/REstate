using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate
{
    public interface IStateMachine<TState, TInput>
    {
        string MachineId { get; }

        Task<State<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            CancellationToken cancellationToken);

        Task<State<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            Guid? lastCommitTag,
            CancellationToken cancellationToken);

        Task<State<TState>> SendAsync(
            TInput input,
            CancellationToken cancellationToken);

        Task<State<TState>> SendAsync(
            TInput input,
            Guid? lastCommitTag,
            CancellationToken cancellationToken);

        Task<bool> IsInStateAsync(State<TState> state, CancellationToken cancellationToken);

        Task<State<TState>> GetCurrentStateAsync(CancellationToken cancellationToken);

        Task<ICollection<TInput>> GetPermittedInputAsync(CancellationToken cancellationToken);
    }
}
