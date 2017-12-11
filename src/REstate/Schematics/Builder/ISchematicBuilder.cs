using System;
using System.Collections.Generic;

namespace REstate.Schematics.Builder
{
    public interface ISchematicBuilder<TState, TInput>
        : ISchematic<TState, TInput>
    {
        ISchematicBuilder<TState, TInput> WithState(TState state, Action<IStateBuilder<TState, TInput>> stateBuilderAction = null);
        ISchematicBuilder<TState, TInput> WithStates(ICollection<TState> states, Action<IStateBuilder<TState, TInput>> stateBuilderAction = null);
        ISchematicBuilder<TState, TInput> WithStates(params TState[] states);

        ISchematicBuilder<TState, TInput> WithTransition(TState sourceState, TInput input, TState resultantState, Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null);

        Schematic<TState, TInput> Build();
    }
}