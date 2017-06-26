using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;

namespace REstate.Engine
{
    /// <summary>
    /// DOT GraphViz text writer for cartographer API.
    /// </summary>
    public class DotGraphCartographer : ICartographer
    {
        /// <summary>
        /// Produces a DOT GraphViz graph.
        /// </summary>
        /// <param name="stateMachine">The StateMachine to be mapped.</param>
        /// <returns>DOT GraphViz text.</returns>
        public string WriteMap(IDictionary<State, StateConfiguration> configuration)
        {

            List<string> lines = new List<string>();

            foreach (var statePair in configuration)
            {

                var source = statePair.Key.StateName;
                foreach (var transition in statePair.Value.Transitions ?? new Transition[0])
                {
                    HandleTransitions(ref lines, source, transition.TriggerName, transition.ResultantStateName, transition.Guard?.Description);
                }
            }

            if (configuration.Values.Any(s => s.OnEntry != null))
            {
                lines.Add(" node [shape=box];");

                foreach (var stateCfg in configuration.Values.Where(s => s.OnEntry != null))
                {
                    var source = stateCfg.StateName;

                    string line = string.Format(" {0} -> \"{1}\" [label=\"On Entry\" style=dotted];", source, stateCfg.OnEntry.Description ?? stateCfg.OnEntry.ConnectorKey);
                    lines.Add(line);
                }
            }

            return "digraph {" + "\r\n" +
                     string.Join("\r\n", lines) + "\r\n" +
                   "}";
        }

        private static void HandleTransitions(ref List<string> lines, string sourceState, string trigger, string destination, string guardDescription)
        {
            string line = string.IsNullOrWhiteSpace(guardDescription) ?
                string.Format(" \"{0}\" -> \"{1}\" [label=\"{2}\"];", sourceState, destination, trigger) :
                string.Format(" \"{0}\" -> \"{1}\" [label=\"{2} [{3}]\"];", sourceState, destination, trigger, guardDescription);

            lines.Add(line);
        }
    }
}