using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Connectors.Console
{
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