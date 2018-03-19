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
        : EventListener
    {
        private readonly LogLevel _loggingLevel;

        private static readonly Lazy<LoggingEventListener> LazyTrace =
            new Lazy<LoggingEventListener>(() => CreateAndStart(LogLevel.Trace));

        private static readonly Lazy<LoggingEventListener> LazyDebug =
            new Lazy<LoggingEventListener>(() => CreateAndStart(LogLevel.Debug));

        private static readonly Lazy<LoggingEventListener> LazyInfo =
            new Lazy<LoggingEventListener>(() => CreateAndStart(LogLevel.Info));

        private static ILog Logger => LogProvider.For<LoggingEventListener>();

        private LoggingEventListener(LogLevel loggingLevel)
            // only process events if log level is enabled.
            : base(() => Logger.Log(loggingLevel, null))
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

        protected override Task OnMachineCreatedAsync<TState, TInput>(
            ISchematic<TState, TInput> schematic, 
            MachineCreationEventData<TState> machineCreationEventData)
        {
            Logger.LogFormat(
                _loggingLevel,
                "Machine {machineId} created in state {state} with commit number of {commitNumber}.",
                machineCreationEventData.InitialStatus.MachineId,
                machineCreationEventData.InitialStatus.State,
                machineCreationEventData.InitialStatus.CommitNumber);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Task OnTransitionAsync<TState, TInput, TPayload>(
            ISchematic<TState, TInput> schematic, 
            Status<TState> status, 
            IReadOnlyDictionary<string, string> metadata,
            TInput input, 
            TPayload payload)
        {
            Logger.LogFormat(
                _loggingLevel,
                "Machine {machineId} transitioned to {state} with input {input}.",
                status.MachineId,
                status.State,
                input,
                payload,
                status.CommitNumber);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        protected override Task OnMachineDeletedAsync(string machineId)
        {
            Logger.LogFormat(
                _loggingLevel,
                "Machine {machineId} deleted.",
                machineId);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }
    }
}
