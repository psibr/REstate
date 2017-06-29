using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate
{
    public interface IStateMachine
    {
        string MachineId { get; }

        Task<MachineStatus> SendAsync(
            Input input,
            string payload, 
            CancellationToken cancellationToken);

        Task<MachineStatus> SendAsync(
            Input input,
            string payload, 
            Guid? lastCommitTag,
            CancellationToken cancellationToken);

        Task<bool> IsInStateAsync(State state, CancellationToken cancellationToken);

        Task<MachineStatus> GetCurrentStateAsync(CancellationToken cancellationToken);

        Task<ICollection<Input>> GetPermittedInputAsync(CancellationToken cancellationToken);
    }
}
