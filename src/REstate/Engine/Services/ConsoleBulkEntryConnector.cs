using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Services
{
    public class ConsoleBulkEntryConnector<TState, TInput>
        : IBulkEntryConnector<TState, TInput>
    {
        private readonly List<string> _lines = new List<string>();

        public Task ExecuteBulkEntryAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var line in _lines)
            {
                Console.WriteLine(line);
            }

            return Task.CompletedTask;
        }

        public Task OnInitialEntryAsync(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machineInstance,
            Status<TState> status,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            const string format = "Machine {{{0}}} entered status {{{1}}}";

            _lines.Add(string.Format(format, machineInstance.MachineId, status.State));

            return Task.CompletedTask;
        }
    }
}