using System.Collections.Generic;
using System.Linq;
using REstate.Schematics;

namespace REstate
{
    public static class SchematicExtensions
    {
        public static Schematic<TState, TInput> Copy<TState, TInput>(this ISchematic<TState, TInput> schematic) => 
            new Schematic<TState, TInput>
            {
                SchematicName = schematic.SchematicName,
                InitialState = schematic.InitialState,
                States = schematic.States.Values.Select(Copy).ToArray()
            };

        public static State<TState, TInput> Copy<TState, TInput>(this IState<TState, TInput> state) =>
            new State<TState, TInput>
            {
                Value = state.Value,
                ParentState = state.ParentState,
                Description = state.Description,
                Transitions = state.Transitions.Values.Select(Copy).ToArray(),
                OnEntry = state.OnEntry?.Copy()
            };

        public static EntryConnector<TInput> Copy<TInput>(this IEntryAction<TInput> entryAction) => 
            new EntryConnector<TInput>
            {
                ConnectorKey = entryAction.ConnectorKey,
                Description = entryAction.Description,
                FailureTransition = new ExceptionTransition<TInput> {Input = entryAction.OnFailureInput},
                Configuration = new Dictionary<string, string>((IDictionary<string, string>) entryAction.Settings)
            };

        public static Transition<TState, TInput> Copy<TState, TInput>(this ITransition<TState, TInput> transition) =>
            new Transition<TState, TInput>
            {
                Input = transition.Input,
                ResultantState = transition.ResultantState,
                Guard = transition.Guard?.Copy()
            };

        public static GuardConnector Copy(this IGuard guard) =>
            new GuardConnector
            {
                ConnectorKey = guard.ConnectorKey,
                Configuration = new Dictionary<string, string>((IDictionary<string, string>)guard.Settings)
            };
    }
}
