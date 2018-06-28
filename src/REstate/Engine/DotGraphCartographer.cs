using System;
using System.Collections.Generic;
using System.Linq;
using REstate.Schematics;

namespace REstate.Engine
{
    /// <summary>
    /// DOT GraphViz text writer for cartographer API.
    /// </summary>
    public class DotGraphCartographer<TState, TInput>
        : ICartographer<TState, TInput>
    {
        private static readonly Lazy<DotGraphCartographer<TState, TInput>> InstanceLazy = new Lazy<DotGraphCartographer<TState,TInput>>();
        public static DotGraphCartographer<TState, TInput> Instance => InstanceLazy.Value;

        /// <summary>
        /// Produces a DOT GraphViz graph.
        /// </summary>
        /// <returns>DOT GraphViz text.</returns>
        public string WriteMap(IEnumerable<IState<TState, TInput>> states)
        {
            var lines = new List<string>();
            var actionLines = new List<string>();

            foreach (var state in states)
            {
                var source = state.Value;

                lines.AddRange(state.Transitions.Values
                    .Select(transition =>
                        GetTransitionRepresentation(
                            source.ToString(),
                            transition.Input.ToString(),
                            transition.ResultantState.ToString(),
                            transition.Precondition?.Description)));

                if (state.Action != null)
                {
                    actionLines.Add($" {state.Value} -> \"{state.Action.Description ?? state.Action.ConnectorKey.Identifier}\"" +
                                            " [label=\"executes\" style=dotted];");
                }
            }

            if (actionLines.Count > 0)
            {
                lines.Add(" node [shape=box];");

                lines.AddRange(actionLines);
            }

            return $"digraph {{\r\n{ string.Join("\r\n\t", lines) }\r\n}}";
        }

        private static string GetTransitionRepresentation(string sourceState, string input, string destination, string preconditionDescription)
        {
            return string.IsNullOrWhiteSpace(preconditionDescription)
                ? $" \"{sourceState}\" -> \"{destination}\" [label=\"{input}\"];"
                : $" \"{sourceState}\" -> \"{destination}\" [label=\"{input} ({preconditionDescription})\"];";
        }
    }
}
