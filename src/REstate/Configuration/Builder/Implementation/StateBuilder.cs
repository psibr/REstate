using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Configuration.Builder.Implementation
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
        public string Description { get; private set; }
        public IDictionary<string, ITransition> Transitions { get; } = new Dictionary<string, ITransition>();

        public IEntryAction OnEntry { get; private set; }

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

            Description = description;

            return this;
        }

        public IStateBuilder WithTransitionTo(string resultantStateName, Input input, Action<ITransitionBuilder> transition = null)
        {
            _builder.WithTransition(StateName, input, resultantStateName, transition);

            return this;
        }

        public IStateBuilder WithTransitionFrom(string previousStateName, Input input, Action<ITransitionBuilder> transition = null)
        {
            _builder.WithTransition(previousStateName, input, StateName, transition);

            return this;
        }

        public IStateBuilder WithReentrance(Input input, Action<ITransitionBuilder> transition = null)
        {
            _builder.WithTransition(StateName, input, StateName, transition);

            return this;
        }

        public IStateBuilder WithOnEntry(string connectorKey, Action<IEntryActionBuilder> onEntry = null)
        {
            if (string.IsNullOrWhiteSpace(connectorKey))
                throw new ArgumentException("ConnectorKey must have a valid value", nameof(connectorKey));

            var onEntryBuilder = new EntryActionBuilder(connectorKey );

            onEntry?.Invoke(onEntryBuilder);

            OnEntry = onEntryBuilder;

            return this;
        }

        public StateConfiguration ToStateConfiguration()
        {
            return new StateConfiguration
            {
                StateName = StateName,
                ParentStateName = ParentStateName,
                Description = Description,
                OnEntry = OnEntry?.ToEntryConnector(),
                Transitions = Transitions.Values.Select(t => t.ToTransition()).ToArray()
            };
        }
    }
}