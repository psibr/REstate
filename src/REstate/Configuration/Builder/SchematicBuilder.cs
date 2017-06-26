using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REstate.Configuration.Builder
{
    public class SchematicBuilder
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

        public IDictionary<string, IStateConfigurationBuilder> StateConfigurations { get; } = new Dictionary<string, IStateConfigurationBuilder>();

        public IDictionary<string, ServiceState> ServiceStates { get; } = new Dictionary<string, ServiceState>();

        public SchematicBuilder WithState(string stateName, Action<IStateConfigurationBuilder> stateBuilder = null)
        {
            var stateConfiguration = new StateConfigurationBuilder(this, stateName);

            stateBuilder?.Invoke(stateConfiguration);

            StateConfigurations.Add(stateConfiguration.StateName, stateConfiguration);

            return this;
        }

        public SchematicBuilder WithStates(IEnumerable<string> stateNames, Action<IStateConfigurationBuilder> stateBuilder = null)
        {
            foreach (var stateName in stateNames)
            {
                var stateConfiguration = new StateConfigurationBuilder(this, stateName);

                stateBuilder?.Invoke(stateConfiguration);

                StateConfigurations.Add(stateConfiguration.StateName, stateConfiguration);
            }

            return this;
        }

        public SchematicBuilder WithTransition(string stateName, string triggerName, string resultantStateName, GuardConnector guard = null)
        {
            if (stateName == null)
                throw new ArgumentNullException(nameof(stateName));
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("No value provided.", nameof(stateName));

            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));
            if (string.IsNullOrWhiteSpace(triggerName))
                throw new ArgumentException("No value provided.", nameof(triggerName));

            if (resultantStateName == null)
                throw new ArgumentNullException(nameof(resultantStateName));
            if (string.IsNullOrWhiteSpace(resultantStateName))
                throw new ArgumentException("No value provided.", nameof(resultantStateName));


            if (!StateConfigurations.ContainsKey(resultantStateName))
                throw new ArgumentException("Resultant state was not defined.", nameof(resultantStateName));

            if (!StateConfigurations.TryGetValue(stateName, out IStateConfigurationBuilder stateBuilder))
            {
                throw new ArgumentException($"No matching start state found.", nameof(stateName));
            }

            try
            {
                stateBuilder.Transitions.Add(triggerName, new Transition
                {
                    TriggerName = triggerName,
                    Guard = guard,
                    ResultantStateName = resultantStateName
                });
            }
            catch(ArgumentException ex)
            {
                throw new InvalidOperationException($"An trigger matching: [ {triggerName} ] is already defined on state: [ {stateName} ]", ex);
            }

            return this;
        }

        public Schematic ToSchematic()
        {
            return new Schematic
            {
                SchematicName = SchematicName,
                InitialState = InitialState,
                StateConfigurations = StateConfigurations.Select(kvp => kvp.Value.ToStateConfiguration()).ToArray(),
                ServiceStates = ServiceStates.Select(kvp => kvp.Value).ToArray()
            };
        }
    }
}
