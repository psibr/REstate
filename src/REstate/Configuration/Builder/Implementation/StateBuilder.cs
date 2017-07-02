using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Configuration.Builder.Implementation
{
    internal class StateBuilder<TState>
        : IStateBuilder<TState>
    {
        private readonly SchematicBuilder<TState> _builder;

        public StateBuilder(SchematicBuilder<TState> builder, TState state)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

            Value = state;
        }

        public TState Value { get; }
        public TState ParentState { get; private set; }
        public string Description { get; private set; }
        public IDictionary<Input, ITransition<TState>> Transitions { get; } = new Dictionary<Input, ITransition<TState>>();

        public IEntryAction OnEntry { get; private set; }

        public IStateBuilder<TState> AsInitialState()
        {
            _builder.SetInitialState(Value);

            return this;
        }

        public IStateBuilder<TState> AsSubStateOf(TState state)
        {
            if (!_builder.States.Keys.Contains(state))
            {
                throw new ArgumentException("Parent state not defined.", nameof(state));
            }

            ParentState = state;

            return this;
        }

        public IStateBuilder<TState> DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descriptions, if provided, should not be empty.", nameof(description));

            Description = description;

            return this;
        }

        public IStateBuilder<TState> WithTransitionTo(TState resultantState, Input input, Action<ITransitionBuilder<TState>> transitionBuilderAction = null)
        {
            _builder.WithTransition(Value, input, resultantState, transitionBuilderAction);

            return this;
        }

        public IStateBuilder<TState> WithTransitionFrom(TState previousState, Input input, Action<ITransitionBuilder<TState>> transitionBuilderAction = null)
        {
            _builder.WithTransition(previousState, input, Value, transitionBuilderAction);

            return this;
        }

        public IStateBuilder<TState> WithReentrance(Input input, Action<ITransitionBuilder<TState>> transition = null)
        {
            _builder.WithTransition(Value, input, Value, transition);

            return this;
        }

        public IStateBuilder<TState> WithOnEntry(string connectorKey, Action<IEntryActionBuilder> onEntry = null)
        {
            if (string.IsNullOrWhiteSpace(connectorKey))
                throw new ArgumentException("ConnectorKey must have a valid value", nameof(connectorKey));

            var onEntryBuilder = new EntryActionBuilder(connectorKey );

            onEntry?.Invoke(onEntryBuilder);

            OnEntry = onEntryBuilder;

            return this;
        }

        public StateConfiguration<TState> ToStateConfiguration()
        {
            return new StateConfiguration<TState>
            {
                Value = Value,
                ParentState = ParentState,
                Description = Description,
                OnEntry = OnEntry?.ToEntryConnector(),
                Transitions = Transitions.Values.Select(t => t.ToTransition()).ToArray()
            };
        }
    }
}