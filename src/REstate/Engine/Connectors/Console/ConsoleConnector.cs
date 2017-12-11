using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Console
{
    public enum ConsoleWriteMode
    {
        Write,
        WriteLine
    }

    public enum ConsoleReadMode
    {
        ReadKey,
        ReadLine
    }

    public class ConsoleEntryConnectorConfiguration
        : IConnectorConfiguration
    {
        public ConsoleEntryConnectorConfiguration(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public ConsoleWriteMode Mode { get; set; } = ConsoleWriteMode.WriteLine;
    }

    public class ConsoleGuardianConnectorConfiguration
        : IConnectorConfiguration
    {
        public ConsoleGuardianConnectorConfiguration(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public ConsoleReadMode Mode { get; set; } = ConsoleReadMode.ReadLine;
    }

    public class ConsoleEntryConnector<TState, TInput>
        : IEntryConnector<TState, TInput>
        , IBulkEntryConnector<TState, TInput>
    {
        public Task OnEntryAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status, 
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            string format = null;
            connectorSettings?.TryGetValue("Format", out format);
            const string machineEnteredStatus = "Machine {0} entered status {1}";
            format = format ?? machineEnteredStatus;

            if(inputParameters != null)
                System.Console.WriteLine(format, machine.MachineId, status.State, inputParameters.Input, inputParameters.Payload);
            else
                System.Console.WriteLine(machineEnteredStatus, machine.MachineId, status.State, null, null);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        public IBulkEntryConnectorBatch<TState, TInput> CreateBatch() => 
            new ConsoleBulkEntryConnectorBatch();

        private class ConsoleBulkEntryConnectorBatch
            : IBulkEntryConnectorBatch<TState, TInput>
        {
            private readonly List<string> _lines = new List<string>();

            public Task OnBulkEntryAsync<TPayload>(
                ISchematic<TState, TInput> schematic,
                IStateMachine<TState, TInput> machine,
                Status<TState> status,
                InputParameters<TInput, TPayload> inputParameters,
                IReadOnlyDictionary<string, string> connectorSettings,
                CancellationToken cancellationToken = new CancellationToken())
            {
                const string format = "Machine {{{0}}} entered status {{{1}}}";

                _lines.Add(string.Format(format, machine.MachineId, status.State));

#if NET45
            return Task.FromResult(0);
#else
                return Task.CompletedTask;
#endif
            }

            public Task ExecuteBulkEntryAsync(CancellationToken cancellationToken = new CancellationToken())
            {
                foreach (var line in _lines)
                {
                    System.Console.WriteLine(line);
                }

#if NET45
            return Task.FromResult(0);
#else
                return Task.CompletedTask;
#endif
            }
        }
    }

    public class ConsoleGuardianConnector<TState, TInput>
        : IGuardianConnector<TState, TInput>
    {
#pragma warning disable 1998
        public async Task<bool> GuardAsync<TPayload>(
#pragma warning restore 1998
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            string prompt = null;
            connectorSettings?.TryGetValue("Prompt", out prompt);
            prompt = prompt ?? "Machine {{{0}}} in status {{{1}}} is attempting to transition with input {{{2}}}, allow? (y/n)";

            while (true)
            {
                System.Console.WriteLine(prompt, machine.MachineId, status.State, inputParameters.Input, inputParameters.Payload);
                var response = System.Console.ReadLine()?.ToLowerInvariant();

                switch (response)
                {
                    case "y":
                        return true;
                    case "n":
                        return false;
                    default:
                        continue;
                }
            }
        }
    }
}
