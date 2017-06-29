using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public interface IStateBuilder
        : IState
    {
        IStateBuilder AsInitialState();

        IStateBuilder AsSubStateOf(string stateName);

        IStateBuilder DescribedAs(string description);

        IStateBuilder WithTransitionTo(string resultantStateName, Input input, Action<ITransitionBuilder> transition = null);

        IStateBuilder WithTransitionFrom(string previousStateName, Input input, Action<ITransitionBuilder> transition = null);

        IStateBuilder WithReentrance(Input input, Action<ITransitionBuilder> transition = null);

        IStateBuilder WithOnEntry(string connectorKey, Action<IEntryActionBuilder> onEntry = null);
    }

    public interface IState
    {
        string StateName { get; }

        string ParentStateName { get; }

        string Description { get; }

        IDictionary<string, ITransition> Transitions { get; }

        IEntryAction OnEntry { get; }

        StateConfiguration ToStateConfiguration();
    }
}