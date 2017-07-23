using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Services
{
    public class ConsoleConnector<TState, TInput>
        : IConnector<TState, TInput>
    {
        string IConnector.ConnectorKey => ConnectorKey;

        public Task OnEntryAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            TInput input,
            TPayload payload,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string format = null;
            connectorSettings?.TryGetValue("Format", out format);
            format = format ?? "Machine {{{0}}} entered status {{{1}}}";

            Console.WriteLine(format, machine.MachineId, status.State, payload);

            return Task.CompletedTask;
        }

        public Task OnInitialEntryAsync(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine, 
            Status<TState> status, 
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = new CancellationToken())
        {
            const string format = "Machine {{{0}}} entered status {{{1}}}";

            Console.WriteLine(format, machine.MachineId, status.State);

            return Task.CompletedTask;
        }

        public async Task<bool> GuardAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            TInput input,
            TPayload payload,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string prompt = null;
            connectorSettings?.TryGetValue("Prompt", out prompt);
            prompt = prompt ?? "Machine {{{0}}} in status {{{1}}} is attempting to transition with input {{{2}}}, allow? (y/n)";

            while (true)
            {
                Console.WriteLine(prompt, machine.MachineId, status.State, input, payload);
                var response = await Task.Run(() => Console.ReadLine()?.ToLowerInvariant(), cancellationToken).ConfigureAwait(false);

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

        public IBulkEntryConnector<TState, TInput> GetBulkEntryConnector() => 
            new ConsoleBulkEntryConnector<TState, TInput>();

        public static string ConnectorKey => "Console";
    }
}
