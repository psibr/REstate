using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Configuration.Builder.Implementation
{
    internal class SchematicBuilder 
        : ISchematicBuilder
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

        public string InitialState { get; private set; }

        internal void SetInitialState(string stateName)
        {
            if (InitialState != null)
                throw new InvalidOperationException("Initial state cannot be set twice.");

            InitialState = stateName;
        }

        private readonly Dictionary<string, IStateBuilder> _stateConfigurations = new Dictionary<string, IStateBuilder>();

        public IReadOnlyDictionary<string, IState> States => 
            _stateConfigurations.ToDictionary(kvp => kvp.Key, kvp => (IState)kvp.Value);

        public ISchematicBuilder WithState(string stateName, Action<IStateBuilder> state = null)
        {
            var stateConfiguration = new StateBuilder(this, stateName);

            _stateConfigurations.Add(stateConfiguration.StateName, stateConfiguration);

            state?.Invoke(stateConfiguration);

            return this;
        }

        public ISchematicBuilder WithStates(ICollection<string> stateNames, Action<IStateBuilder> state = null)
        {
            foreach (var stateName in stateNames)
            {
                var stateConfiguration = new StateBuilder(this, stateName);

                _stateConfigurations.Add(stateConfiguration.StateName, stateConfiguration);
            }

            foreach (var stateName in stateNames)
            {
                var stateConfiguration = _stateConfigurations[stateName];

                state?.Invoke(stateConfiguration);
            }

            return this;
        }

        public ISchematicBuilder WithStates(params string[] stateNames)
        {
            foreach (var stateName in stateNames)
            {
                var stateConfiguration = new StateBuilder(this, stateName);

                _stateConfigurations.Add(stateConfiguration.StateName, stateConfiguration);
            }

            return this;
        }

        public ISchematicBuilder WithTransition(string stateName, Input input, string resultantStateName, Action<ITransitionBuilder> transition = null)
        {
            if (stateName == null)
                throw new ArgumentNullException(nameof(stateName));
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("No value provided.", nameof(stateName));

            if (resultantStateName == null)
                throw new ArgumentNullException(nameof(resultantStateName));
            if (string.IsNullOrWhiteSpace(resultantStateName))
                throw new ArgumentException("No value provided.", nameof(resultantStateName));


            if (!_stateConfigurations.ContainsKey(resultantStateName))
                throw new ArgumentException("Resultant state was not defined.", nameof(resultantStateName));

            if (_stateConfigurations.TryGetValue(stateName, out IStateBuilder stateBuilder))
            {
                try
                {
                    var transitionBuilder = new TransitionBuilder(input, resultantStateName);

                    transition?.Invoke(transitionBuilder);

                    stateBuilder.Transitions.Add(input, transitionBuilder);
                }
                catch (ArgumentException ex)
                {
                    throw new InvalidOperationException($"Input matching: [ {input} ] is already defined on state: [ {stateName} ]", ex);
                }
            }
            else
            {
                throw new ArgumentException("No matching start state found.", nameof(stateName));
            }

            return this;
        }

        public Schematic ToSchematic()
        {
            return new Schematic
            {
                SchematicName = SchematicName,
                InitialState = InitialState,
                StateConfigurations = _stateConfigurations.Values.Select(builder => builder.ToStateConfiguration()).ToArray()
            };
        }
    }
}
