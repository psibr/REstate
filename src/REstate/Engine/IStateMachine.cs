using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Engine
{
    public interface IStateMachine
    {
        string MachineId { get; }

        Task<Machine> FireAsync(
            Trigger trigger,
            string payload, 
            CancellationToken cancellationToken);

        Task<Machine> FireAsync(
            Trigger trigger,
            string payload, 
            Guid? lastCommitTag,
            CancellationToken cancellationToken);

        Task<bool> IsInStateAsync(State state, CancellationToken cancellationToken);

        Task<Machine> GetCurrentStateAsync(CancellationToken cancellationToken);

        Task<ICollection<Trigger>> GetPermittedTriggersAsync(CancellationToken cancellationToken);
    }
}
