using REstate.Engine.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REstate.Engine;
using System.Threading;

namespace REstate.Services
{
    public class ConsoleWriterConnector 
        : IConnector
    {
        string IConnector.ConnectorKey => ConnectorKey;

        public Func<CancellationToken, Task> ConstructAction(IStateMachine machineInstance, State state, string payload, IDictionary<string, string> configuration) =>
            (cancellationToken) =>
            {
                string format = null;
                configuration?.TryGetValue("Format", out format);
                format = format ?? "Entered state: {0}";

                System.Console.WriteLine(format, state.StateName);

                return Task.CompletedTask;
            };
        

        public Func<State, Trigger, string, CancellationToken, Task<bool>> ConstructPredicate(IStateMachine machineInstance, IDictionary<string, string> configuration)
        {
            throw new NotImplementedException();
        }

        public static string ConnectorKey => "ConsoleWriter";
    }
}
