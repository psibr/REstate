using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryMachineStatusStore<TState, TInput>
        : IMachineStatusStore<TState, TInput>
    {
        private readonly Lazy<Task<MachineStatus<TState, TInput>>> _machineStatusAsyncLazy;

        public InMemoryMachineStatusStore(InMemoryRepositoryContextFactory<TState, TInput> repositoryContextFactory, string machineId)
        {
            _machineStatusAsyncLazy = new Lazy<Task<MachineStatus<TState, TInput>>>(() =>
            {
                using (var ctx = repositoryContextFactory.OpenContextAsync().Result)
                {
                    return ((EngineRepository<TState, TInput>)ctx.Machines).GetMachineStatusAsync(machineId, CancellationToken.None);
                }
            });
        }

        /// <inheritdoc />
        public Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(CancellationToken cancellationToken = default)
        {
            return _machineStatusAsyncLazy.Value;
        }

        /// <inheritdoc />
        public Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            TState state,
            long? lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default)
        {
            lock (_machineStatusAsyncLazy.Value)
            {
                if (lastCommitNumber is null || _machineStatusAsyncLazy.Value.Result.CommitNumber == lastCommitNumber)
                {
                    _machineStatusAsyncLazy.Value.Result.State = state;
                    _machineStatusAsyncLazy.Value.Result.CommitNumber++;
                    _machineStatusAsyncLazy.Value.Result.UpdatedTime = DateTimeOffset.UtcNow;

                    if (stateBag != null && lastCommitNumber != null)
                        _machineStatusAsyncLazy.Value.Result.StateBag = stateBag;
                }
                else
                {
                    throw new StateConflictException();
                }
            }

            return _machineStatusAsyncLazy.Value;
        }
    }
}
