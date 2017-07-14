using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Services
{
    public class ConsoleConnector<TState, TInput>
        : IConnector<TState, TInput>
    {
        string IConnector.ConnectorKey => ConnectorKey;

        public Task OnEntryAsync<TPayload>(IStateMachine<TState, TInput> machineInstance, State<TState> state, TInput input, TPayload payload, IDictionary<string, string> connectorSettings, CancellationToken cancellationToken)
        {
            string format = null;
            connectorSettings?.TryGetValue("Format", out format);
            format = format ?? "Machine {{{0}}} entered state {{{1}}}";

            Console.WriteLine(format, machineInstance.MachineId, state.Value, payload);

            return Task.CompletedTask;
        }

        public async Task<bool> GuardAsync<TPayload>(IStateMachine<TState, TInput> machineInstance, State<TState> state, TInput input, TPayload payload, IDictionary<string, string> connectorSettings, CancellationToken cancellationToken)
        {
            string prompt = null;
            connectorSettings?.TryGetValue("Prompt", out prompt);
            prompt = prompt ?? "Machine {{{0}}} in state {{{1}}} is attempting to transition with input {{{2}}}, allow? (y/n)";

            while (true)
            {
                Console.WriteLine(prompt, machineInstance.MachineId, state.Value, input, payload);
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

        public static string ConnectorKey => "Console";
    }
}
