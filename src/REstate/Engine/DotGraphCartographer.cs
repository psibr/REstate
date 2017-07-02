using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;

namespace REstate.Engine
{
    /// <summary>
    /// DOT GraphViz text writer for cartographer API.
    /// </summary>
    public class DotGraphCartographer<TState>
        : ICartographer<TState>
    {
        /// <summary>
        /// Produces a DOT GraphViz graph.
        /// </summary>
        /// <returns>DOT GraphViz text.</returns>
        public string WriteMap(IDictionary<State<TState>, StateConfiguration<TState>> configuration)
        {
            var lines = new List<string>();

            foreach (var statePair in configuration)
            {

                var source = statePair.Key.Value;
                foreach (var transition in statePair.Value.Transitions ?? new Transition<TState>[0])
                {
                    HandleTransitions(ref lines, source.ToString(), transition.InputName, transition.ResultantState.ToString(), transition.Guard?.Description);
                }
            }

            if (configuration.Values.Any(s => s.OnEntry != null))
            {
                lines.Add(" node [shape=box];");

                lines.AddRange(configuration.Values
                    .Where(s => s.OnEntry != null)
                    .Select(state =>
                        $" {state.Value} -> \"{state.OnEntry.Description ?? state.OnEntry.ConnectorKey}\" [label=\"On Entry\" style=dotted];"));
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