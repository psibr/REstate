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
                OnEntry = state.OnEntry?.Clone()
            };

        public static EntryConnector<TInput> Clone<TInput>(this IEntryAction<TInput> entryAction) => 
            new EntryConnector<TInput>
            {
                ConnectorKey = entryAction.ConnectorKey,
                Description = entryAction.Description,
                ExceptionInput = (entryAction.OnExceptionInput != null) 
                    ? new ExceptionInput<TInput>(entryAction.OnExceptionInput.Input)
                    : null,
                Configuration = new Dictionary<string, string>((IDictionary<string, string>) entryAction.Settings)
            };

        public static Transition<TState, TInput> Clone<TState, TInput>(this ITransition<TState, TInput> transition) =>
            new Transition<TState, TInput>
            {
                Input = transition.Input,
                ResultantState = transition.ResultantState,
                Guard = transition.Guard?.Clone()
            };

        public static GuardConnector Clone(this IGuard guard) =>
            new GuardConnector
            {
                ConnectorKey = guard.ConnectorKey,
                Configuration = new Dictionary<string, string>((IDictionary<string, string>)guard.Settings)
            };
    }
}
