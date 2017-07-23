using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.IoC;
using REstate.Schematics;

namespace REstate
{
    public interface IEventListener
    {
        Task OnMachineCreated<TState, TInput>(
            IEnumerable<IStateMachine<TState, TInput>> machines,
            ISchematic<TState, TInput> schematic,
            Status<TState> initialStatus,
            CancellationToken cancellation = default(CancellationToken));

        Task OnTransition<TState, TInput, TPayload>(
            IStateMachine<TState, TInput> machine,
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
