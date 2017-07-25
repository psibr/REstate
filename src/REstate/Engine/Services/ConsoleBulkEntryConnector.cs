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

        public Task OnEntryAsync<TPayload>(
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
                Console.WriteLine(line);
            }

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }
    }
}