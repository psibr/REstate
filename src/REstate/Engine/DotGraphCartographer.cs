using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;

namespace REstate.Engine
{
    /// <summary>
    /// DOT GraphViz text writer for cartographer API.
    /// </summary>
    public class DotGraphCartographer 
        : ICartographer
    {
        /// <summary>
        /// Produces a DOT GraphViz graph.
        /// </summary>
        /// <returns>DOT GraphViz text.</returns>
        public string WriteMap(IDictionary<State, StateConfiguration> configuration)
        {
            var lines = new List<string>();

            foreach (var statePair in configuration)
            {

                var source = statePair.Key.StateName;
                foreach (var transition in statePair.Value.Transitions ?? new Transition[0])
                {
                    HandleTransitions(ref lines, source, transition.InputName, transition.ResultantStateName, transition.Guard?.Description);
                }
            }

            if (configuration.Values.Any(s => s.OnEntry != null))
            {
                lines.Add(" node [shape=box];");

                lines.AddRange(configuration.Values
                    .Where(s => s.OnEntry != null)
                    .Select(state =>
                        $" {state.StateName} -> \"{state.OnEntry.Description ?? state.OnEntry.ConnectorKey}\" [label=\"On Entry\" style=dotted];"));
            }

            return "digraph {" + "\r\n" +
                     string.Join("\r\n", lines) + "\r\n" +
                   "}";
        }

        private static void HandleTransitions(ref List<string> lines, string sourceState, Input input, string destination, string guardDescription)
        {
            var line = string.IsNullOrWhiteSpace(guardDescription) 
                ? $" \"{sourceState}\" -> \"{destination}\" [label=\"{input}\"];" 
                : $" \"{sourceState}\" -> \"{destination}\" [label=\"{input} ({guardDescription})\"];";

            lines.Add(line);
        }
    }
}