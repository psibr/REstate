using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Natural
{
    public class NaturalDotGraphCartographer : INaturalCartographer
    {
        
        private static readonly Lazy<NaturalDotGraphCartographer> InstanceLazy = new Lazy<NaturalDotGraphCartographer>();
     
        /// <summary>
        /// A static instance of this cartographer so that it can be used for .ToString functionality.
        /// <para />
        /// Perfer resolving <see cref="INaturalCartographer"/> from Container when feasible.
        /// </summary>
        public static NaturalDotGraphCartographer Instance => InstanceLazy.Value;

        /// <summary>
        /// Produces a DOT GraphViz graph.
        /// </summary>
        /// <returns>DOT GraphViz text.</returns>
        public string WriteMap(INaturalSchematic schematic)
        {
            var lines = new List<string>();
            var actionLines = new List<string>();

            foreach (var state in schematic.States.Values)
            {
                var source = state.Value;

                lines.AddRange(state.Transitions.Values
                    .Select(transition =>
                        GetTransitionRepresentation(
                            source.ToString(),
                            transition.Input.ToString(),
                            transition.ResultantState.ToString(),
                            transition.Precondition?.Description)));

                if (state.Action != null && state.Value.ToType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAcceptSignal<>)))
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