using System;
using REstate.Engine.Connectors;

namespace REstate.Schematics.Builder.Providers
{
    public interface IStateBuilderProvider<TState, TInput, out TThis>
        : IState<TState, TInput>
    {
        TThis AsInitialState();

        TThis AsSubstateOf(TState state);

        TThis DescribedAs(string description);

        TThis WithTransitionTo(TState resultantState, TInput input, System.Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null);

        TThis WithTransitionFrom(TState previousState, TInput input, System.Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null);

        TThis WithReentrance(TInput input, System.Action<ITransitionBuilder<TState, TInput>> transition = null);

        TThis WithAction(ConnectorKey connectorKey, System.Action<IActionBuilder<TInput>> action = null);

        TThis WithAction<TConnector>(System.Action<IActionBuilder<TInput>> action = null) where TConnector : IAction;
    }
}
