using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface IOptimisticallyConcurrentMachineStatusStore<TState, TInput>
    {
        Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            MachineStatus<TState, TInput> machineStatus,
            TState state,
            long? lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default);
    }
}
