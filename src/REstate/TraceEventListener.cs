using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate;
using REstate.Logging;
using REstate.Schematics;

namespace REstate
{
    public class TraceEventListener
        : IEventListener
    {
        private static ILog Logger => LogProvider.For<TraceEventListener>();

        Task IEventListener.OnMachineCreated<TState, TInput>(
            ISchematic<TState, TInput> schematic,
            ICollection<Status<TState>> initialStatuses,
            CancellationToken cancellationToken)
        {
            foreach (var status in initialStatuses)
            {
                Logger.TraceFormat(
                    "Machine {machineId} created in state {state} with commit tag of {commitTag}.",
                    status.MachineId,
                    status.State,
                    status.CommitTag);
            }

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        Task IEventListener.OnTransition<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            TInput input,
            TPayload payload,
            CancellationToken cancellation)
        {
            Logger.TraceFormat("Machine {machineId} transitioned to {state} with input {input}.",
                status.MachineId,
                status.State,
                input,
                payload,
                status.CommitTag);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        Task IEventListener.OnMachineDeleted(
            IEnumerable<string> machineIds,
            CancellationToken cancellation)
        {
            foreach (var machineId in machineIds)
            {
                Logger.TraceFormat(
                    "Machine {machineId} deleted.",
                    machineId);
            }

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }
    }
}