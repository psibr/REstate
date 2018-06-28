using REstate.Schematics.Builder.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Schematics.Builder
{
    public interface ISchematicBuilder<TState, TInput>
    : ISchematicBuilderProvider<TState, TInput, ISchematicBuilder<TState, TInput>>
    {

    }

    public class SchematicBuilder<TState, TInput>
        : ISchematicBuilder<TState, TInput>
    {
        private bool _hasInitialState;

        public SchematicBuilder(string schematicName)
        {
            if (schematicName == null)
                throw new ArgumentNullException(nameof(schematicName));
            if (string.IsNullOrWhiteSpace(schematicName))
                throw new ArgumentException("No value provided.", nameof(schematicName));

            SchematicName = schematicName;
        }

        public string SchematicName { get; }

        public TState InitialState { get; private set; }

        public int StateConflictRetryCount { get; private set; }

        internal void SetInitialState(TState state)
        {
            InitialState = state;
            _hasInitialState = true;
        }

        private readonly Dictionary<TState, IStateBuilder<TState, TInput>> _stateConfigurations
            = new Dictionary<TState, IStateBuilder<TState, TInput>>();

        public IReadOnlyDictionary<TState, IState<TState, TInput>> States =>
            _stateConfigurations.ToDictionary(kvp => kvp.Key, kvp => (IState<TState, TInput>)kvp.Value);

        public ISchematicBuilder<TState, TInput> WithState(
            TState state,
            System.Action<IStateBuilder<TState, TInput>> stateBuilderAction = null)
        {
            var stateConfiguration = new StateBuilder<TState, TInput>(this, state);

            _stateConfigurations.Add(stateConfiguration.Value, stateConfiguration);

            stateBuilderAction?.Invoke(stateConfiguration);

            return this;
        }

        public ISchematicBuilder<TState, TInput> WithStates(
            ICollection<TState> states,
            System.Action<IStateBuilder<TState, TInput>> stateBuilderAction = null)
        {
            foreach (var state in states)
            {
                var stateConfiguration = new StateBuilder<TState, TInput>(this, state);

                _stateConfigurations.Add(state, stateConfiguration);
            }

            foreach (var state in states)
            {
                var stateConfiguration = _stateConfigurations[state];

                stateBuilderAction?.Invoke(stateConfiguration);
            }

            return this;
        }

        public ISchematicBuilder<TState, TInput> WithStates(params TState[] states)
        {
            foreach (var state in states)
            {
                var stateConfiguration = new StateBuilder<TState, TInput>(this, state);

                _stateConfigurations.Add(state, stateConfiguration);
            }

            return this;
        }

        public ISchematicBuilder<TState, TInput> WithTransition(
            TState sourceState,
            TInput input,
            TState resultantState,
            System.Action<ITransitionBuilder<TState, TInput>> transition = null)
        {
            if (!_stateConfigurations.ContainsKey(resultantState))
                throw new ArgumentException("Resultant stateBuilderAction was not defined.", nameof(resultantState));

            if (_stateConfigurations.TryGetValue(sourceState, out var stateBuilder))
            {
                try
                {
                    var transitionBuilder = new TransitionBuilder<TState, TInput>(input, resultantState);

                    transition?.Invoke(transitionBuilder);

                    stateBuilder.Transitions.Add(input, transitionBuilder);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException($"Input matching: [ {input} ] is already defined on state: [ {sourceState} ]", ex);
                }
            }
            else
            {
                throw new ArgumentException("No matching start stateBuilderAction found.", nameof(sourceState));
            }

            return this;
        }

        public Schematic<TState, TInput> Build()
        {
            if (!_hasInitialState)
                throw new ArgumentException("Cannot build a schematic with no initial state.");

            var schematic = this as ISchematic<TState, TInput>;

            return new Schematic<TState, TInput>(
                schematic.SchematicName,
                schematic.InitialState,
                schematic.StateConflictRetryCount,
                schematic.States.Values
                    .Select(SchematicCloner.Clone)
                    .ToArray());

        }

        public ISchematicBuilder<TState, TInput> WithStateConflictRetries()
        {
            return WithStateConflictRetries(-1);
        }

        public ISchematicBuilder<TState, TInput> WithStateConflictRetries(int retryCount)
        {
            if (retryCount < -1)
                throw new ArgumentException("Values below -1 are not valid.", nameof(retryCount));

            StateConflictRetryCount = retryCount;

            return this;
        }
    }
}
