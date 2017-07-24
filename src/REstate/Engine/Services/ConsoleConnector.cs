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
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string format = null;
            connectorSettings?.TryGetValue("Format", out format);
            const string machineEnteredStatus = "Machine {{{0}}} entered status {{{1}}}";
            format = format ?? machineEnteredStatus;

            if(inputParameters != null)
                Console.WriteLine(format, machine.MachineId, status.State, inputParameters.Input, inputParameters.Payload);
            else
                Console.WriteLine(machineEnteredStatus, machine.MachineId, status.State, null, null);

            return Task.CompletedTask;
        }

        public async Task<bool> GuardAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string prompt = null;
            connectorSettings?.TryGetValue("Prompt", out prompt);
            prompt = prompt ?? "Machine {{{0}}} in status {{{1}}} is attempting to transition with input {{{2}}}, allow? (y/n)";

            while (true)
            {
                Console.WriteLine(prompt, machine.MachineId, status.State, inputParameters.Input, inputParameters.Payload);
                var response = Console.ReadLine()?.ToLowerInvariant();

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
