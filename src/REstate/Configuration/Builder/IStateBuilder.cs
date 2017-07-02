using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public interface IStateBuilder<TState>
        : IState<TState>
    {
        IStateBuilder<TState> AsInitialState();

        IStateBuilder<TState> AsSubStateOf(TState state);

        IStateBuilder<TState> DescribedAs(string description);

        IStateBuilder<TState> WithTransitionTo(TState resultantState, Input input, Action<ITransitionBuilder<TState>> transitionBuilderAction = null);

        IStateBuilder<TState> WithTransitionFrom(TState previousState, Input input, Action<ITransitionBuilder<TState>> transitionBuilderAction = null);

        IStateBuilder<TState> WithReentrance(Input input, Action<ITransitionBuilder<TState>> transition = null);

        IStateBuilder<TState> WithOnEntry(string connectorKey, Action<IEntryActionBuilder> onEntry = null);
    }

    public interface IState<TState>
    {
        TState Value { get; }

        TState ParentState { get; }

        string Description { get; }

        IDictionary<Input, ITransition<TState>> Transitions { get; }

        IEntryAction OnEntry { get; }

        StateConfiguration<TState> ToStateConfiguration();
    }
}