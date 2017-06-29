using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public interface IStateBuilder
    {
        string StateName { get; }

        string ParentStateName { get; }

        string StateDescription { get; }
        
        IDictionary<string, Transition> Transitions { get; }

        IStateBuilder AsInitialState();

        IStateBuilder AsSubStateOf(string stateName);

        IStateBuilder DescribedAs(string description);

        IStateBuilder WithTransitionTo(string resultantStateName, Input input, GuardConnector guard = null);

        IStateBuilder WithTransitionFrom(string previousStateName, Input input, GuardConnector guard = null);

        IStateBuilder WithOnEntry(string connectorKey, Action<IEntryActionBuilder> onEntryBuilder);

        StateConfiguration ToStateConfiguration();
    }
}