using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Configuration.Builder.Implementation
{
    internal class SchematicBuilder<TState>
        : ISchematicBuilder<TState>
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

        private readonly Dictionary<TState, IStateBuilder<TState>> _stateConfigurations = new Dictionary<TState, IStateBuilder<TState>>();

        public IReadOnlyDictionary<TState, IState<TState>> States => 
            _stateConfigurations.ToDictionary(kvp => kvp.Key, kvp => (IState<TState>)kvp.Value);

        public ISchematicBuilder<TState> WithState(TState state, Action<IStateBuilder<TState>> stateBuilderAction = null)
        {
            var stateConfiguration = new StateBuilder<TState>(this, state);

            _stateConfigurations.Add(stateConfiguration.Value, stateConfiguration);

            stateBuilderAction?.Invoke(stateConfiguration);

            return this;
        }

        public ISchematicBuilder<TState> WithStates(ICollection<TState> states, Action<IStateBuilder<TState>> stateBuilderAction = null)
        {
            foreach (var state in states)
            {
                var stateConfiguration = new StateBuilder<TState>(this, state);

                _stateConfigurations.Add(state, stateConfiguration);
            }

            foreach (var state in states)
            {
                var stateConfiguration = _stateConfigurations[state];

                stateBuilderAction?.Invoke(stateConfiguration);
            }

            return this;
        }

        public ISchematicBuilder<TState> WithStates(params TState[] states)
        {
            foreach (var state in states)
            {
                var stateConfiguration = new StateBuilder<TState>(this, state);

                _stateConfigurations.Add(state, stateConfiguration);
            }

            return this;
        }

        public ISchematicBuilder<TState> WithTransition(TState sourceState, Input input, TState resultantState, Action<ITransitionBuilder<TState>> transition = null)
        {
            if (!_stateConfigurations.ContainsKey(resultantState))
                throw new ArgumentException("Resultant stateBuilderAction was not defined.", nameof(resultantState));

            if (_stateConfigurations.TryGetValue(sourceState, out IStateBuilder<TState> stateBuilder))
            {
                try
                {
                    var transitionBuilder = new TransitionBuilder<TState>(input, resultantState);

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

        public Schematic<TState> ToSchematic()
        {
            return new Schematic<TState>
            {
                SchematicName = SchematicName,
                InitialState = InitialState,
                StateConfigurations = _stateConfigurations.Values.Select(builder => builder.ToStateConfiguration()).ToArray()
            };
        }
    }
}
