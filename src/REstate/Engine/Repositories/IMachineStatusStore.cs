using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface IMachineStatusStore<TState, TInput>
    {
        /// <summary>
        /// Updates the Status record of a Machine
        /// </summary>
        /// <param name="state">The state to which the Status is set.</param>
        /// <param name="lastCommitNumber">
        /// If provided, will guarantee the update will occur only 
        /// if the value matches the current Status's CommitNumber.
        /// </param>
        /// <param name="stateBag">A dictionary of string keys and values to associate with the transition</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="MachineDoesNotExistException">Thrown when no matching MachineId was found.</exception>
        /// <exception cref="StateConflictException">Thrown when a conflict occured on CommitNumber; no update was performed.</exception>
        Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            TState state,
            long? lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the record for a Machine Status.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="MachineDoesNotExistException">Thrown when no matching MachineId was found.</exception>
        Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(CancellationToken cancellationToken = default);
    }
}
