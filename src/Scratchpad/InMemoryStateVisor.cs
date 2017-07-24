using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate;
using REstate.Schematics;

namespace Scratchpad
{
    public class InMemoryStateVisor
        : IEventListener
    {
        private readonly Dictionary<string, (string SchematicName, Stack<object> States)> _machineStates = new Dictionary<string, (string, Stack<object>)>();

        Task IEventListener.OnMachineCreated<TState, TInput>(
            ISchematic<TState, TInput> schematic,
            ICollection<Status<TState>> initialStatuses,
            CancellationToken cancellationToken)
        {
            foreach (var status in initialStatuses)
            {
                _machineStates.Add(status.MachineId, (schematic.SchematicName, new Stack<object>(new object[] { status })));
            }

            return Task.CompletedTask;
        }

        Task IEventListener.OnTransition<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            TInput input,
            TPayload payload,
            CancellationToken cancellation)
        {
            _machineStates[status.MachineId].States.Push(status);

            return Task.CompletedTask;
        }

        Task IEventListener.OnMachineDeleted(
            IEnumerable<string> machineIds,
            CancellationToken cancellation)
        {
            foreach (var machineId in machineIds)
            {
                _machineStates.Remove(machineId);
            }

            return Task.CompletedTask;
        }

        public Status<TState> GetStatus<TState>(string machineId) =>
            (Status<TState>)_machineStates[machineId].States.Peek();

        public Status<TState> GetStatus<TState, TInput>(IStateMachine<TState, TInput> machine) =>
            (Status<TState>)_machineStates[machine.MachineId].States.Peek();
    }
}