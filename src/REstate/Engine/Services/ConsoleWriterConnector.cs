using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Configuration;

namespace REstate.Engine.Services
{
    public class ConsoleWriterConnector
        : IConnector
    {
        string IConnector.ConnectorKey => ConnectorKey;

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, State state, string payload, IDictionary<string, string> connectorSettings) =>
            cancellationToken =>
            {
                string format = null;
                connectorSettings?.TryGetValue("Format", out format);
                format = format ?? "machine with Id: {0} entered state: {1}";

                Console.WriteLine(format, machineInstance.MachineId, state.StateName, payload);

                return Task.CompletedTask;
            };


        public Func<State, Input, string, CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, IDictionary<string, string> connectorSettings)
        {
            throw new NotSupportedException();
        }

        public static string ConnectorKey => "ConsoleWriter";
    }
}
