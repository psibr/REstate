using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Console
{
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
}
