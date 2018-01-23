using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.EventListeners
{
    /// <summary>
    /// A base implementation of <see cref="IEventListener"/> that attempts to prioritize messages on event time.
    /// </summary>
    public abstract class EventListener
        : IEventListener
    {
        private static readonly BlockingCollection<(DateTimeOffset eventTime, Func<Task> asyncAction)> EventQueue =
            new BlockingCollection<(DateTimeOffset, Func<Task>)>(
                new ConcurrentPriorityQueue<DateTimeOffset, Func<Task>>());

        private readonly Func<bool> _preCondition;

        protected EventListener(Func<bool> preCondition)
        {
            this._preCondition = preCondition;
        }

        /// <summary>
        /// Instructs the listener to begin its monitoring.
        /// </summary>
        /// <param name="shutdownCancellationToken">
        /// A <see cref="CancellationToken"/> that signals the application is shutting-down.
        /// </param>
        public Task StartListenerAsync(CancellationToken shutdownCancellationToken)
        {
            return Task.Run(async () =>
            {
                var lastCount = 0;

                while (!shutdownCancellationToken.IsCancellationRequested)
                {
                    if (lastCount != 0)
                    {
                        for (int i = 0; i < lastCount; i++)
                        {
                            if (EventQueue.TryTake(out var nextEventItem))
                            {
                                await nextEventItem.asyncAction();
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(100, shutdownCancellationToken);
                    }

                    lastCount = EventQueue.Count;
                }
            }, CancellationToken.None);
        }

        Task IEventListener.MachineCreatedAsync<TState, TInput>(
            ISchematic<TState, TInput> schematic,
            ICollection<MachineCreationEventData<TState>> initialStatuses)
        {
            if(_preCondition())
                foreach (var machineInfo in initialStatuses)
                    EventQueue.Add(
                        item: (
                        eventTime: machineInfo.InitialStatus.UpdatedTime,
                        asyncAction: () => OnMachineCreatedAsync(schematic, machineInfo)),
                        cancellationToken: CancellationToken.None);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        protected abstract Task OnMachineCreatedAsync<TState, TInput>(
            ISchematic<TState, TInput> schematic,
            MachineCreationEventData<TState> machineCreationEventData);

        Task IEventListener.TransitionAsync<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            IReadOnlyDictionary<string, string> metadata,
            TInput input,
            TPayload payload)
        {
            if(_preCondition())
                EventQueue.Add(
                    item: (
                    eventTime: status.UpdatedTime,
                    asyncAction: () => OnTransitionAsync(schematic, status, metadata, input, payload)),
                    cancellationToken: CancellationToken.None);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        protected abstract Task OnTransitionAsync<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            IReadOnlyDictionary<string, string> metadata,
            TInput input,
            TPayload payload);

        Task IEventListener.MachineDeletedAsync(
            DateTimeOffset deletionTime,
            IEnumerable<string> machineIds)
        {
            if(_preCondition())
                foreach (var machineId in machineIds)
                    EventQueue.Add(
                        item: (
                        eventTime: DateTime.UtcNow,
                        asyncAction: () => OnMachineDeletedAsync(machineId)),
                        cancellationToken: CancellationToken.None);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        protected abstract Task OnMachineDeletedAsync(string machineId);
    }
}