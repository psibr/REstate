using REstate.Engine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Natural
{
    public interface INaturalStateMachine
    {
        string MachineId { get; }

        Task<INaturalSchematic> GetSchematicAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<string, string>> GetMetadataAsync(
            CancellationToken cancellationToken = default);

        Task<Status<TypeState>> GetCurrentStateAsync(
            CancellationToken cancellationToken = default);

        Task<Status<TypeState>> SignalAsync<TSignal>(
            TSignal signal,
            CancellationToken cancellationToken = default);

        Task<Status<TypeState>> SignalAsync<TSignal>(
            TSignal signal,
            long lastCommitNumber,
            CancellationToken cancellationToken = default);
    }

    public class NaturalStateMachine : INaturalStateMachine
    {
        private readonly IStateMachine<TypeState, TypeState> _stateMachine;

        public NaturalStateMachine(IStateMachine<TypeState, TypeState> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public string MachineId => _stateMachine.MachineId;

        public Task<Status<TypeState>> GetCurrentStateAsync(CancellationToken cancellationToken = default) =>
            _stateMachine.GetCurrentStateAsync(cancellationToken);

        public Task<IReadOnlyDictionary<string, string>> GetMetadataAsync(CancellationToken cancellationToken = default) =>
            _stateMachine.GetMetadataAsync(cancellationToken);

        public async Task<INaturalSchematic> GetSchematicAsync(CancellationToken cancellationToken = default)
        {
            var schematic = await _stateMachine.GetSchematicAsync(cancellationToken).ConfigureAwait(false);

            return new NaturalSchematic(schematic);
        }

        public Task<Status<TypeState>> SignalAsync<TSignal>(TSignal signal, CancellationToken cancellationToken = default)
        {
            return _stateMachine.SendAsync(typeof(TSignal), signal, cancellationToken);
        }

        public Task<Status<TypeState>> SignalAsync<TSignal>(TSignal signal, long lastCommitNumber, CancellationToken cancellationToken = default)
        {
            return _stateMachine.SendAsync(typeof(TSignal), signal, lastCommitNumber, cancellationToken);
        }

        public override string ToString()
        {
            return _stateMachine.ToString();
        }
    }
}
