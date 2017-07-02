using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public interface ISchematicBuilder<TState>
        : ISchematic<TState>
    { 
        ISchematicBuilder<TState> WithState(TState state, Action<IStateBuilder<TState>> stateBuilderAction = null);
        ISchematicBuilder<TState> WithStates(ICollection<TState> states, Action<IStateBuilder<TState>> stateBuilderAction = null);
        ISchematicBuilder<TState> WithStates(params TState[] states);

        ISchematicBuilder<TState> WithTransition(TState sourceState, Input input, TState resultantState, Action<ITransitionBuilder<TState>> transitionBuilderAction = null);
    }

    public interface ISchematic<TState>
    {
        string SchematicName { get; }
        TState InitialState { get; }
        IReadOnlyDictionary<TState, IState<TState>> States { get; }

        Schematic<TState> ToSchematic();
    }
}