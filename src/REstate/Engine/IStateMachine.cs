using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine
{
    public interface IStateMachine<TState, TInput>
    {
        Task<ISchematic<TState, TInput>> GetSchematicAsync(
            CancellationToken cancellationToken = default);

        string MachineId { get; }

        Task<IReadOnlyDictionary<string, string>> GetMetadataAsync(
            CancellationToken cancellationToken = default);

        Task<Status<TState>> GetCurrentStateAsync(
            CancellationToken cancellationToken = default);

        Task<Status<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload,
            CancellationToken cancellationToken = default);

        Task<Status<TState>> SendAsync<TPayload>(
            TInput input,
            TPayload payload, 
            long lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default);

        Task<Status<TState>> SendAsync(
            TInput input,
            CancellationToken cancellationToken = default);

        Task<Status<TState>> SendAsync(
            TInput input,
            long lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default);
    }
}
