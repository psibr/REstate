using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Configuration.Builder
{
    public interface IStateConfigurationBuilder
    {
        string StateName { get; }

        string ParentStateName { get; }

        string StateDescription { get; }

        EntryConnector OnEntry { get; }
        
        IDictionary<string, Transition> Transitions { get; }

        IStateConfigurationBuilder AsInitialState();

        IStateConfigurationBuilder AsSubStateOf(string stateName);

        IStateConfigurationBuilder DescribedAs(string description);

        IStateConfigurationBuilder WithTransition(string triggerName, string resultantStateName, GuardConnector guard = null);

        IStateConfigurationBuilder WithOnEntryConnector(EntryConnector onEntry);

        StateConfiguration ToStateConfiguration();
    }

    public class StateConfigurationBuilder : IStateConfigurationBuilder
    {
        private SchematicBuilder _builder;

        public StateConfigurationBuilder(SchematicBuilder builder, string stateName)
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
        public EntryConnector OnEntry { get; private set; }

        public IStateConfigurationBuilder AsInitialState()
        {
            _builder.SetInitialState(StateName);

            return this;
        }

        public IStateConfigurationBuilder AsSubStateOf(string stateName)
        {
            if (stateName == null)
                throw new ArgumentNullException(nameof(stateName));
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("No value provided.", nameof(stateName));

            if (!_builder.StateConfigurations.Keys.Contains(stateName)
                && !_builder.ServiceStates.Keys.Contains(stateName))
            {
                throw new ArgumentException("Parent state not defined.", nameof(stateName));
            }

            ParentStateName = stateName;

            return this;
        }

        public IStateConfigurationBuilder DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descriptions, if provided, should not be empty.", nameof(description));

            StateDescription = description;

            return this;
        }

        public IStateConfigurationBuilder WithTransition(string triggerName, string resultantStateName, GuardConnector guard = null)
        {
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));
            if (string.IsNullOrWhiteSpace(triggerName))
                throw new ArgumentException("No value provided.", nameof(triggerName));

            if (resultantStateName == null)
                throw new ArgumentNullException(nameof(resultantStateName));
            if (string.IsNullOrWhiteSpace(resultantStateName))
                throw new ArgumentException("No value provided.", nameof(resultantStateName));

            try
            {
                Transitions.Add(triggerName, new Transition
                {
                    TriggerName = triggerName,
                    Guard = guard,
                    ResultantStateName = resultantStateName
                });
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"An trigger matching: [ {triggerName} ] is already defined on state: [ {StateName} ]", ex);
            }

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
                Transitions = Transitions.Select(kvp => kvp.Value).ToArray()
            }; 
        }

        public IStateConfigurationBuilder WithOnEntryConnector(EntryConnector onEntry)
        {
            if (onEntry == null)
                throw new ArgumentNullException(nameof(onEntry));

            if (string.IsNullOrWhiteSpace(onEntry.ConnectorKey))
                throw new ArgumentException("ConnectorKey must have a valid value", nameof(onEntry));

            OnEntry = onEntry;

            return this;
        }
    }
}