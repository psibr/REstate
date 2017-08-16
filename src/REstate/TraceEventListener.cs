using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Logging;
using REstate.Schematics;

namespace REstate
{
    public class TraceEventListener
        : IEventListener
    {
        public TraceEventListener()
        {
            Task.Run(() =>
            {
                foreach (var action in LoggerQueue.GetConsumingEnumerable())
                {
                    action();
                }
            });
        }

        private static readonly BlockingCollection<Action> LoggerQueue = 
            new BlockingCollection<Action>(new ConcurrentQueue<Action>());

        private static ILog Logger => LogProvider.For<TraceEventListener>();

        Task IEventListener.OnMachineCreated<TState, TInput>(
            ISchematic<TState, TInput> schematic,
            ICollection<(Status<TState> Status, IReadOnlyDictionary<string, string> Metadata)> initialStatuses,
            CancellationToken cancellationToken)
        {
            foreach (var machineInfo in initialStatuses)
            {
                var status = machineInfo.Status;

                LoggerQueue.Add(() =>
                    Logger.TraceFormat(
                        "Machine {machineId} created in state {state} with commit tag of {commitTag}.",
                        status.MachineId,
                        status.State,
                        status.CommitTag), CancellationToken.None);
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
            IReadOnlyDictionary<string, string> metadata,
            TInput input,
            TPayload payload,
            CancellationToken cancellation)
        {
            LoggerQueue.Add(() =>
                Logger.TraceFormat("Machine {machineId} transitioned to {state} with input {input}.",
                    status.MachineId,
                    status.State,
                    input,
                    payload,
                    status.CommitTag), CancellationToken.None);

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
                LoggerQueue.Add(() =>
                    Logger.TraceFormat(
                        "Machine {machineId} deleted.",
                        machineId), CancellationToken.None);
            }

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }
    }
}