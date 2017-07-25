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
            ICollection<Status<TState>> initialStatuses,
            CancellationToken cancellation = default(CancellationToken));

        Task OnTransition<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            TInput input,
            TPayload payload,
            CancellationToken cancellation = default(CancellationToken));

        Task OnMachineDeleted(
            IEnumerable<string> machineIds,
            CancellationToken cancellation = default(CancellationToken));
    }
}
