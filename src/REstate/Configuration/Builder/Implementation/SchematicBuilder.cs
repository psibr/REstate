using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Configuration.Builder.Implementation
{
    internal class SchematicBuilder<TState, TInput>
        : ISchematicBuilder<TState, TInput>
    {
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

        internal void SetInitialState(TState state)
        {
            if (InitialState != null)
                throw new InvalidOperationException("Initial stateBuilderAction cannot be set twice.");

            InitialState = state;
        }

        private readonly Dictionary<TState, IStateBuilder<TState, TInput>> _stateConfigurations = new Dictionary<TState, IStateBuilder<TState, TInput>>();

        public IReadOnlyDictionary<TState, IState<TState, TInput>> States => 
            _stateConfigurations.ToDictionary(kvp => kvp.Key, kvp => (IState<TState, TInput>)kvp.Value);

        public ISchematicBuilder<TState, TInput> WithState(TState state, Action<IStateBuilder<TState, TInput>> stateBuilderAction = null)
        {
            var stateConfiguration = new StateBuilder<TState, TInput>(this, state);

            _stateConfigurations.Add(stateConfiguration.Value, stateConfiguration);

            stateBuilderAction?.Invoke(stateConfiguration);

            return this;
        }

        public ISchematicBuilder<TState, TInput> WithStates(ICollection<TState> states, Action<IStateBuilder<TState, TInput>> stateBuilderAction = null)
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

        public ISchematicBuilder<TState, TInput> WithTransition(TState sourceState, TInput input, TState resultantState, Action<ITransitionBuilder<TState, TInput>> transition = null)
        {
            if (!_stateConfigurations.ContainsKey(resultantState))
                throw new ArgumentException("Resultant stateBuilderAction was not defined.", nameof(resultantState));

            if (_stateConfigurations.TryGetValue(sourceState, out IStateBuilder<TState, TInput> stateBuilder))
            {
                try
                {
                    var transitionBuilder = new TransitionBuilder<TState, TInput>(input, resultantState);

                    transition?.Invoke(transitionBuilder);

                    stateBuilder.Transitions.Add(input, transitionBuilder);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException($"Input matching: [ {input} ] is already defined on stateBuilderAction: [ {sourceState} ]", ex);
                }
            }
            else
            {
                throw new ArgumentException("No matching start stateBuilderAction found.", nameof(sourceState));
            }

            return this;
        }

        public Schematic<TState, TInput> ToSchematic()
        {
            return new Schematic<TState, TInput>
            {
                SchematicName = SchematicName,
                InitialState = InitialState,
                StateConfigurations = _stateConfigurations.Values.Select(builder => builder.ToStateConfiguration()).ToArray()
            };
        }
    }
}
