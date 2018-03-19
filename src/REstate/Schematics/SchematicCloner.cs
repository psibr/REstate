using System.Collections.Generic;
using System.Linq;

namespace REstate.Schematics
{
    public static class SchematicCloner
    {
        public static Schematic<TState, TInput> Clone<TState, TInput>(this ISchematic<TState, TInput> schematic) =>
            new Schematic<TState, TInput>(
                schematic.SchematicName,
                schematic.InitialState,
                schematic.StateConflictRetryCount,
                schematic.States.Values
                    .Select(Clone)
                    .ToArray());

        public static State<TState, TInput> Clone<TState, TInput>(this IState<TState, TInput> state) =>
            new State<TState, TInput>
            {
                Value = state.Value,
                ParentState = state.ParentState,
                Description = state.Description,
                Transitions = state.Transitions.Values.Select(Clone).ToArray(),
                Action = state.Action?.Clone()
            };

        public static Action<TInput> Clone<TInput>(this IAction<TInput> action) => 
            new Action<TInput>
            {
                ConnectorKey = action.ConnectorKey,
                Description = action.Description,
                ExceptionInput = (action.OnExceptionInput != null) 
                    ? new ExceptionInput<TInput>(action.OnExceptionInput.Input)
                    : null,
                Configuration = new Dictionary<string, string>((IDictionary<string, string>) action.Settings)
            };

        public static Transition<TState, TInput> Clone<TState, TInput>(this ITransition<TState, TInput> transition) =>
            new Transition<TState, TInput>
            {
                Input = transition.Input,
                ResultantState = transition.ResultantState,
                Precondition = transition.Procondition?.Clone()
            };

        public static Precondition Clone(this IPrecondition precondition) =>
            new Precondition
            {
                ConnectorKey = precondition.ConnectorKey,
                Configuration = new Dictionary<string, string>((IDictionary<string, string>)precondition.Settings)
            };
    }
}
