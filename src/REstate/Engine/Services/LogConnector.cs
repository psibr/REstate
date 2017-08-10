using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Logging;
using REstate.Schematics;

namespace REstate.Engine.Services
{
    public class LogConnector<TState, TInput>
        : IConnector<TState, TInput>
    {
        private static ILog Logger => LogProvider.For<LogConnector<TState, TInput>>();

        string IConnector.ConnectorKey => ConnectorKey;

        public Task OnEntryAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string format = null;
            connectorSettings?.TryGetValue("message", out format);
            const string machineEnteredStatus = "Machine {machineId} entered state: {state}.";
            format = format ?? machineEnteredStatus;

            var logLevel = LogLevel.Info;

            string severity = null;
            if (connectorSettings?.TryGetValue("severity", out severity) ?? false)
            {
                Enum.TryParse(severity, true, out logLevel);
            }

            if(inputParameters != null)
                Logger.Log(logLevel, () => format, null, machine.MachineId, status.State, inputParameters.Input, inputParameters.Payload);
            else
                Logger.Log(logLevel, () => format, null, machine.MachineId, status.State, null, null);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

#pragma warning disable 1998
        public async Task<bool> GuardAsync<TPayload>(
#pragma warning restore 1998
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken)) =>
                throw new NotSupportedException();

        public IBulkEntryConnector<TState, TInput> GetBulkEntryConnector() =>
            throw new NotSupportedException();

        public static string ConnectorKey => "Log";
    }
}
