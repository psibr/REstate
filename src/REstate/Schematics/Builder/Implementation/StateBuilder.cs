using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Schematics.Builder.Implementation
{
    internal class StateBuilder<TState, TInput>
        : IStateBuilder<TState, TInput>
    {
        private readonly SchematicBuilder<TState, TInput> _builder;

        public StateBuilder(SchematicBuilder<TState, TInput> builder, TState state)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

            Value = state;
        }

        public TState Value { get; }
        public TState ParentState { get; private set; }
        public string Description { get; private set; }
        public IDictionary<TInput, ITransition<TState, TInput>> Transitions { get; } = new Dictionary<TInput, ITransition<TState, TInput>>();

        public IEntryAction<TInput> OnEntry { get; private set; }

        public IStateBuilder<TState, TInput> AsInitialState()
        {
            _builder.SetInitialState(Value);

            return this;
        }

        public IStateBuilder<TState, TInput> AsSubstateOf(TState state)
        {
            if (!_builder.States.Keys.Contains(state))
            {
                throw new ArgumentException("Parent state not defined.", nameof(state));
            }

            ParentState = state;

            return this;
        }

        public IStateBuilder<TState, TInput> DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descriptions, if provided, should not be empty.", nameof(description));

            Description = description;

            return this;
        }

        public IStateBuilder<TState, TInput> WithTransitionTo(TState resultantState, TInput input, Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null)
        {
            _builder.WithTransition(Value, input, resultantState, transitionBuilderAction);

            return this;
        }

        public IStateBuilder<TState, TInput> WithTransitionFrom(TState previousState, TInput input, Action<ITransitionBuilder<TState, TInput>> transitionBuilderAction = null)
        {
            _builder.WithTransition(previousState, input, Value, transitionBuilderAction);

            return this;
        }

        public IStateBuilder<TState, TInput> WithReentrance(TInput input, Action<ITransitionBuilder<TState, TInput>> transition = null)
        {
            _builder.WithTransition(Value, input, Value, transition);

            return this;
        }

        public IStateBuilder<TState, TInput> WithOnEntry(ConnectorKey connectorKey, Action<IEntryActionBuilder<TInput>> onEntry = null)
        {
            if (connectorKey == null)
                throw new ArgumentNullException(nameof(connectorKey));
            if (string.IsNullOrWhiteSpace(connectorKey.Name))
                throw new ArgumentException("ConnectorKey must have a valid value", nameof(connectorKey));

            var onEntryBuilder = new EntryActionBuilder<TInput>(connectorKey );

            onEntry?.Invoke(onEntryBuilder);

            OnEntry = onEntryBuilder;

            return this;
        }
    }
}