using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using REstate.Logging;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Logger
{
    public class LoggerEntryConnector<TState, TInput>
        : IEntryConnector<TState, TInput>
    {
        private readonly Regex _messageFormatTokensRegex = new Regex(@"(?<=\{).*?(?=\})", RegexOptions.Compiled);

        private readonly IEnumerable<LoggerEntryConnectorConfiguration> _logConfigurations;

        public LoggerEntryConnector(IEnumerable<LoggerEntryConnectorConfiguration> logConfigurations)
        {
            _logConfigurations = logConfigurations;
        }

        private static ILog Logger => LogProvider.For<LoggerEntryConnector<TState, TInput>>();

        public Task OnEntryAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            var configuration = _logConfigurations.FindConfiguration(schematic, status);

            string messageFormat = null;
            connectorSettings?.TryGetValue("messageFormat", out messageFormat);
            const string machineEnteredStateMessage = "Machine {machineId} entered state: {state}.";
            messageFormat = messageFormat ?? machineEnteredStateMessage;

            var logLevel = MapLogLevel(configuration.LogLevel);

            object input = null;
            object payload = null;

            if (inputParameters != null)
            {
                input = inputParameters.Input;
                payload = inputParameters.Payload;
            }

            var logParameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(schematic.SchematicName)] = schematic.SchematicName,
                [nameof(machine.MachineId)] = machine.MachineId,
                [nameof(status.State)] = status.State,
                [nameof(inputParameters.Input)] = input,
                [nameof(inputParameters.Payload)] = payload
            };

            // Get the distinct set of matches including casing.
            var matches = _messageFormatTokensRegex
                .Matches(messageFormat).Cast<Match>()
                .Select(match => match.Value)
                .ToList();

            var parameters = matches.Select(match =>
            {
                logParameters.TryGetValue(match, out var value);

                return value;
            }).ToArray();

            Logger.LogFormat(logLevel, messageFormat, parameters);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        private static Logging.LogLevel MapLogLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return Logging.LogLevel.Trace;
                case LogLevel.Debug:
                    return Logging.LogLevel.Debug;
                case LogLevel.Info:
                    return Logging.LogLevel.Info;
                case LogLevel.Warn:
                    return Logging.LogLevel.Warn;
                case LogLevel.Error:
                    return Logging.LogLevel.Error;
                case LogLevel.Fatal:
                    return Logging.LogLevel.Fatal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
    }
}
