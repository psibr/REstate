using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Configuration.Builder
{
    internal class StateBuilder
        : IStateBuilder
    {
        private readonly SchematicBuilder _builder;

        public StateBuilder(SchematicBuilder builder, string stateName)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

            if (stateName == null)
                throw new ArgumentNullException(nameof(stateName));
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("No value provided.", nameof(stateName));

            StateName = stateName;
        }

        public string StateName { get; }
        public string ParentStateName { get; private set; }
        public string StateDescription { get; private set; }
        public IDictionary<string, Transition> Transitions { get; } = new Dictionary<string, Transition>();

        private EntryConnector OnEntry { get; set; }

        public IStateBuilder AsInitialState()
        {
            _builder.SetInitialState(StateName);

            return this;
        }

        public IStateBuilder AsSubStateOf(string stateName)
        {
            if (stateName == null)
                throw new ArgumentNullException(nameof(stateName));
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("No value provided.", nameof(stateName));

            if (!_builder.States.Keys.Contains(stateName))
            {
                throw new ArgumentException("Parent state not defined.", nameof(stateName));
            }

            ParentStateName = stateName;

            return this;
        }

        public IStateBuilder DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descriptions, if provided, should not be empty.", nameof(description));

            StateDescription = description;

            return this;
        }

        public IStateBuilder WithTransitionTo(string resultantStateName, Input input, GuardConnector guard = null)
        {
            _builder.WithTransition(StateName, input, resultantStateName, guard);

            return this;
        }

        public IStateBuilder WithTransitionFrom(string previousStateName, Input input, GuardConnector guard = null)
        {
            _builder.WithTransition(previousStateName, input, StateName, guard);

            return this;
        }

        public IStateBuilder WithOnEntry(string connectorKey, Action<IEntryActionBuilder> onEntryBuilderAction)
        {
            if (string.IsNullOrWhiteSpace(connectorKey))
                throw new ArgumentException("ConnectorKey must have a valid value", nameof(connectorKey));

            var onEntryBuilder = new EntryActionBuilder(this, connectorKey );

            onEntryBuilderAction?.Invoke(onEntryBuilder);

            OnEntry = onEntryBuilder.ToEntryConnector();

            return this;
        }

        public StateConfiguration ToStateConfiguration()
        {
            return new StateConfiguration
            {
                StateName = StateName,
                ParentStateName = ParentStateName,
                Description = StateDescription,
                OnEntry = OnEntry,
                Transitions = Transitions.Values.ToArray()
            };
        }
    }
}