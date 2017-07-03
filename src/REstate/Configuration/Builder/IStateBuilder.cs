using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public interface IStateBuilder<TState, TInput>
        : IState<TState, TInput>
    {
        IStateBuilder<TState, TInput> AsInitialState();

        IStateBuilder<TState, TInput> AsSubStateOf(TState state);

        IStateBuilder<TState, TInput> DescribedAs(string description);

        IStateBuilder<TState, TInput> WithTransitionTo(TState resultantState, TInput input, Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null);

        IStateBuilder<TState, TInput> WithTransitionFrom(TState previousState, TInput input, Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null);

        IStateBuilder<TState, TInput> WithReentrance(TInput input, Action<ITransitionBuilder<TState, TInput>> transition = null);

        IStateBuilder<TState, TInput> WithOnEntry(string connectorKey, Action<IEntryActionBuilder<TInput>> onEntry = null);
    }

    public interface IState<TState, TInput>
    {
        TState Value { get; }

        TState ParentState { get; }

        string Description { get; }

        IDictionary<TInput, ITransition<TState, TInput>> Transitions { get; }

        IEntryAction<TInput> OnEntry { get; }

        StateConfiguration<TState, TInput> ToStateConfiguration();
    }
}