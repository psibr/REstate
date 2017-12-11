using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Logging;
using REstate.Schematics;

namespace REstate.Engine.EventListeners
{
    /// <summary>
    /// An <see cref="IEventListener"/> that writes events as log messages.
    /// </summary>
    public class LoggingEventListener
        : IEventListener
    {
        private readonly LogLevel _loggingLevel;

        private static readonly BlockingCollection<Action> LoggerQueue =
            new BlockingCollection<Action>(
                new ConcurrentQueue<Action>());

        private static readonly Lazy<LoggingEventListener> LazyTrace = 
            new Lazy<LoggingEventListener>(() => CreateAndStart(LogLevel.Trace));

        private static readonly Lazy<LoggingEventListener> LazyDebug = 
            new Lazy<LoggingEventListener>(() => CreateAndStart(LogLevel.Debug));

        private static readonly Lazy<LoggingEventListener> LazyInfo = 
            new Lazy<LoggingEventListener>(() => CreateAndStart(LogLevel.Info));

        private static ILog Logger => LogProvider.For<LoggingEventListener>();

        private LoggingEventListener(LogLevel loggingLevel)
        {
            _loggingLevel = loggingLevel;
        }

        /// <summary>
        /// A <see cref="LoggingEventListener"/> that writes to the Trace/Verbose logging level.
        /// </summary>
        public static LoggingEventListener Trace => LazyTrace.Value;

        /// <summary>
        /// A <see cref="LoggingEventListener"/> that writes to the Debug logging level.
        /// </summary>
        public static LoggingEventListener Debug => LazyDebug.Value;

        /// A <see cref="LoggingEventListener"/> that writes to the Info/Informational logging level.
        public static LoggingEventListener Informational => LazyInfo.Value;

        private static LoggingEventListener CreateAndStart(LogLevel loggingLevel)
        {
            var listener = new LoggingEventListener(loggingLevel);

            listener.StartListenerAsync(CancellationToken.None);

            return listener;
        }

        /// <summary>
        /// Instructs the listener to begin its monitoring.
        /// </summary>
        /// <param name="shutdownCancellationToken">
        /// A <see cref="CancellationToken"/> that signals the application is shutting-down.
        /// </param>
        public Task StartListenerAsync(CancellationToken shutdownCancellationToken)
        {
            return Task.Run(() =>
            {
                foreach (var action in LoggerQueue.GetConsumingEnumerable())
                {
                    action();
                }
            }, CancellationToken.None);
        }

        Task IEventListener.OnMachineCreatedAsync<TState, TInput>(
            ISchematic<TState, TInput> schematic,
            ICollection<MachineCreationEventData<TState>> initialStatuses)
        {
            if (Logger.Log(_loggingLevel, null))
                foreach (var machineInfo in initialStatuses)
                    LoggerQueue.Add(
                        item: () =>
                            Logger.LogFormat(
                                _loggingLevel,
                                "Machine {machineId} created in state {state} with commit tag of {commitTag}.",
                                machineInfo.InitialStatus.MachineId,
                                machineInfo.InitialStatus.State,
                                machineInfo.InitialStatus.CommitTag),
                        cancellationToken: CancellationToken.None);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        Task IEventListener.OnTransitionAsync<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic,
            Status<TState> status,
            IReadOnlyDictionary<string, string> metadata,
            TInput input,
            TPayload payload)
        {
            if (Logger.Log(_loggingLevel, null))
                LoggerQueue.Add(
                    item: () => Logger.LogFormat(
                        _loggingLevel,
                        "Machine {machineId} transitioned to {state} with input {input}.",
                        status.MachineId,
                        status.State,
                        input,
                        payload,
                        status.CommitTag),
                    cancellationToken: CancellationToken.None);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        Task IEventListener.OnMachineDeletedAsync(
            IEnumerable<string> machineIds)
        {
            if (Logger.Log(_loggingLevel, null))
                foreach (var machineId in machineIds)
                    LoggerQueue.Add(
                        item: () =>
                            Logger.LogFormat(
                                _loggingLevel,
                                "Machine {machineId} deleted.",
                                machineId),
                        cancellationToken: CancellationToken.None);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }
    }
}