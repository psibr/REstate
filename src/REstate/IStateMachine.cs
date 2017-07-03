using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate
{
    public interface IStateMachine<TState, TInput>
    {
        string MachineId { get; }

        Task<State<TState>> SendAsync(
            TInput input,
            string payload, 
            CancellationToken cancellationToken);

        Task<State<TState>> SendAsync(
            TInput input,
            string payload, 
            Guid? lastCommitTag,
            CancellationToken cancellationToken);

        Task<bool> IsInStateAsync(State<TState> state, CancellationToken cancellationToken);

        Task<State<TState>> GetCurrentStateAsync(CancellationToken cancellationToken);

        Task<ICollection<TInput>> GetPermittedInputAsync(CancellationToken cancellationToken);
    }
}
