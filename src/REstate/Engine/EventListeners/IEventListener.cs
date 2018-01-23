using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.EventListeners
{
    /// <summary>
    /// Allows for implementation of classes that will be asynchronously notified of key events.
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        /// Instructs the listener to begin its monitoring.
        /// </summary>
        /// <param name="shutdownCancellationToken">
        /// A <see cref="CancellationToken"/> that signals the application is shutting-down.
        /// </param>
        Task StartListenerAsync(CancellationToken shutdownCancellationToken);

        Task MachineCreatedAsync<TState, TInput>(
            ISchematic<TState, TInput> schematic,
            ICollection<MachineCreationEventData<TState>> initialStatuses);

        Task TransitionAsync<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            IReadOnlyDictionary<string, string> metadata,
            TInput input,
            TPayload payload);

        Task MachineDeletedAsync(
            DateTimeOffset deletionTime,
            IEnumerable<string> machineIds);
    }
}
