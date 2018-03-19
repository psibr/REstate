using System.Collections.Generic;

namespace REstate.Schematics.Builder.Providers
{
    /// <summary>
    /// Low-level interface to allow contravariance (polymorphic return type) in the fluent interface.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TThis">The fluent return type</typeparam>
    public interface ISchematicBuilderProvider<TState, TInput, out TThis>
        : ISchematic<TState, TInput>
    {
        TThis WithStateConflictRetries();
        TThis WithStateConflictRetries(int retryCount);
        TThis WithState(TState state, System.Action<IStateBuilder<TState, TInput>> stateBuilderAction = null);
        TThis WithStates(ICollection<TState> states, System.Action<IStateBuilder<TState, TInput>> stateBuilderAction = null);
        TThis WithStates(params TState[] states);

        TThis WithTransition(TState sourceState, TInput input, TState resultantState, System.Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null);

        Schematic<TState, TInput> Build();
    }
}
