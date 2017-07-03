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

        public Func<CancellationToken, Task> ConstructAction(IStateMachine<TState, TInput> machineInstance, State<TState> state, string payload, IDictionary<string, string> connectorSettings) =>
            cancellationToken =>
            {
                string format = null;
                connectorSettings?.TryGetValue("Format", out format);
                format = format ?? "Machine {{{0}}} entered state {{{1}}}";

                Console.WriteLine(format, machineInstance.MachineId, state.Value, payload);

                return Task.CompletedTask;
            };


        public Func<State<TState>, TInput, string, CancellationToken, Task<bool>> ConstructPredicate(IStateMachine<TState, TInput> machineInstance, IDictionary<string, string> connectorSettings) => 
            (state, input, payload, cancellationToken) =>
            {
                string prompt = null;
                connectorSettings?.TryGetValue("Prompt", out prompt);
                prompt = prompt ?? "Machine {{{0}}} in state {{{1}}} is attempting to transition with input {{{2}}}, allow? (y/n)";

                while (true)
                {
                    Console.WriteLine(prompt, machineInstance.MachineId, state.Value, input, payload);
                    var response = Console.ReadLine()?.ToLowerInvariant();

                    switch (response)
                    {
                        case "y":
                            return Task.FromResult(true);
                        case "n":
                            return Task.FromResult(false);
                        default:
                            continue;
                    }
                }
            };

        public static string ConnectorKey => "Console";
    }
}
