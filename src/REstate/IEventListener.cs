using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate
{
    public interface IEventListener
    {
        Task OnMachineCreated<TState, TInput>(
            ISchematic<TState, TInput> schematic,
            ICollection<(Status<TState> Status, IReadOnlyDictionary<string, string> Metadata)> initialStatuses,
            CancellationToken cancellation = default);

        Task OnTransition<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            IReadOnlyDictionary<string, string> metadata,
            TInput input,
            TPayload payload,
            CancellationToken cancellation = default);

        Task OnMachineDeleted(
            IEnumerable<string> machineIds,
            CancellationToken cancellation = default);
    }
}
