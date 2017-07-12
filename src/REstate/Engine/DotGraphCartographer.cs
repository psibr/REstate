using System.Collections.Generic;
using System.Linq;
using REstate.Configuration;

namespace REstate.Engine
{
    /// <summary>
    /// DOT GraphViz text writer for cartographer API.
    /// </summary>
    public class DotGraphCartographer<TState, TInput>
        : ICartographer<TState, TInput>
    {
        /// <summary>
        /// Produces a DOT GraphViz graph.
        /// </summary>
        /// <returns>DOT GraphViz text.</returns>
        public string WriteMap(ICollection<StateConfiguration<TState, TInput>> configuration)
        {
            var lines = new List<string>();

            foreach (var statePair in configuration)
            {

                var source = statePair.Value;
                foreach (var transition in statePair.Transitions ?? new Transition<TState, TInput>[0])
                {
                    HandleTransitions(ref lines, source.ToString(), transition.Input.ToString(), transition.ResultantState.ToString(), transition.Guard?.Description);
                }
            }

            if (configuration.Any(s => s.OnEntry != null))
            {
                lines.Add(" node [shape=box];");

                lines.AddRange(configuration
                    .Where(s => s.OnEntry != null)
                    .Select(state =>
                        $" {state.Value} -> \"{state.OnEntry.Description ?? state.OnEntry.ConnectorKey}\" [label=\"On Entry\" style=dotted];"));
            }

            return "digraph {" + "\r\n" +
                     string.Join("\r\n", lines) + "\r\n" +
                   "}";
        }

        private static void HandleTransitions(ref List<string> lines, string sourceState, string input, string destination, string guardDescription)
        {
            var line = string.IsNullOrWhiteSpace(guardDescription) 
                ? $" \"{sourceState}\" -> \"{destination}\" [label=\"{input}\"];" 
                : $" \"{sourceState}\" -> \"{destination}\" [label=\"{input} ({guardDescription})\"];";

            lines.Add(line);
        }
    }
}