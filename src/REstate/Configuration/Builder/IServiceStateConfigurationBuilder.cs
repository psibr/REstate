using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Configuration.Builder
{
    public interface IServiceStateConfigurationBuilder
    {
        string StateName { get; }

        string ParentStateName { get; }

        string StateDescription { get; }

        bool UseAcceptAndRejectStates { get; }

        long? RetryDelaySeconds { get; }

        ServiceEntryConnector OnEntry { get; }

        IDictionary<string, Transition> Transitions { get; }

        IServiceStateConfigurationBuilder WithTransition(string triggerName, string resultantStateName, GuardConnector guard = null);

        IServiceStateConfigurationBuilder AsInitialState();

        IServiceStateConfigurationBuilder AsSubStateOf(string stateName);

        IServiceStateConfigurationBuilder DescribedAs(string description);

        IServiceStateConfigurationBuilder UseAcceptAndReject();

        IServiceStateConfigurationBuilder DoNotUseAcceptAndReject();
    }

    public class ServiceStateConfigurationBuilder 
        : IServiceStateConfigurationBuilder
    {
        private SchematicBuilder _builder;

        public ServiceStateConfigurationBuilder(SchematicBuilder builder, string stateName)
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

        public ServiceEntryConnector OnEntry { get; private set; }

        public IDictionary<string, Transition> Transitions { get; } = new Dictionary<string, Transition>();

        public bool UseAcceptAndRejectStates { get; private set; }

        public long? RetryDelaySeconds { get; private set; }

        public IServiceStateConfigurationBuilder WithTransition(string triggerName, string resultantStateName, GuardConnector guard = null)
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

        public IServiceStateConfigurationBuilder AsInitialState()
        {
            _builder.SetInitialState(StateName);

            return this;
        }

        public IServiceStateConfigurationBuilder AsSubStateOf(string stateName)
        {
            if (stateName == null)
                throw new ArgumentNullException(nameof(stateName));
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("No value provided.", nameof(stateName));

            if(!_builder.StateConfigurations.Keys.Contains(stateName) 
                && !_builder.ServiceStates.Keys.Contains(stateName))
            {
                throw new ArgumentException("Parent state not defined.", nameof(stateName));
            }

            ParentStateName = stateName;

            return this;
        }

        public IServiceStateConfigurationBuilder DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descriptions, if provided, should not be empty.", nameof(description));

            StateDescription = description;

            return this;
        }

        public IServiceStateConfigurationBuilder UseAcceptAndReject()
        {
            UseAcceptAndRejectStates = true;

            return this;
        }

        public IServiceStateConfigurationBuilder DoNotUseAcceptAndReject()
        {
            UseAcceptAndRejectStates = false;

            return this;
        }

        public ServiceState ToServiceState()
        {
            return new ServiceState
            {
                StateName = StateName,
                ParentStateName = ParentStateName,
                Description = StateDescription,
                OnEntry = OnEntry,
                UseAcceptAndRejectStates = UseAcceptAndRejectStates,
                RetryDelaySeconds = RetryDelaySeconds,
                Transitions = Transitions.Select(kvp => kvp.Value).ToArray()
            };
        }
    }
}