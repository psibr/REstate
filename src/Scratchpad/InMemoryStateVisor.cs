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
            IEnumerable<IStateMachine<TState, TInput>> machines,
            ISchematic<TState, TInput> schematic,
            Status<TState> initialStatus,
            CancellationToken cancellationToken)
        {
            foreach (var machine in machines)
            {
                _machineStates.Add(machine.MachineId, (schematic.SchematicName, new Stack<object>(new object[] { initialStatus })));
            }

            return Task.CompletedTask;
        }

        Task IEventListener.OnTransition<TState, TInput, TPayload>(
            IStateMachine<TState, TInput> machine,
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            TInput input,
            TPayload payload,
            CancellationToken cancellation)
        {
            _machineStates[machine.MachineId].States.Push(status);

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