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
            IStateMachine<TState, TInput> machine,
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
            string machineId,
            CancellationToken cancellation = default(CancellationToken));
    }
}
