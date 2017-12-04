using System;

namespace REstate.Schematics.Builder
{
    public interface IStateBuilder<TState, TInput>
        : IState<TState, TInput>
    {
        IStateBuilder<TState, TInput> AsInitialState();

        IStateBuilder<TState, TInput> AsSubstateOf(TState state);

        IStateBuilder<TState, TInput> DescribedAs(string description);

        IStateBuilder<TState, TInput> WithTransitionTo(TState resultantState, TInput input, Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null);

        IStateBuilder<TState, TInput> WithTransitionFrom(TState previousState, TInput input, Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null);

        IStateBuilder<TState, TInput> WithReentrance(TInput input, Action<ITransitionBuilder<TState, TInput>> transition = null);

        IStateBuilder<TState, TInput> WithOnEntry(ConnectorKey connectorKey, Action<IEntryActionBuilder<TInput>> onEntry = null);
    }
}