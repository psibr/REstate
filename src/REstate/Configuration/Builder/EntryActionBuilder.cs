using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public class EntryActionBuilder 
        : IEntryActionBuilder
    {
        private readonly IStateBuilder _stateBuilder;

        public EntryActionBuilder(IStateBuilder stateBuilder, string connectorKey)
        {
            if (connectorKey == null)
                throw new ArgumentNullException(nameof(connectorKey));
            if (string.IsNullOrWhiteSpace(connectorKey))
                throw new ArgumentException("Value cannot be empty or whitespace.", nameof(connectorKey));

            ConnectorKey = connectorKey;
            _stateBuilder = stateBuilder ?? throw new ArgumentNullException(nameof(stateBuilder));
        }

        public string ConnectorKey { get; }
        public string Description { get; private set; }
        public string OnFailureInput { get; private set; }

        public IReadOnlyDictionary<string, string> Settings => _settings;

        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();

        public IEntryActionBuilder DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description, if provided, cannot be empty or whitespace.", nameof(description));

            Description = description;

            return this;
        }

        public IEntryActionBuilder WithSetting(string key, string value)
        {
            try
            {
                _settings.Add(key, value);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException($"Setting with key matching: [ {key} ] is already defined.", ex);
            }

            return this;
        }

        public IEntryActionBuilder WithSetting(KeyValuePair<string, string> setting) => 
            WithSetting(setting.Key, setting.Value);

        public IEntryActionBuilder WithSetting((string, string) setting) => 
            WithSetting(setting.Item1, setting.Item2);

        public IEntryActionBuilder OnFailureSend(Input input)
        {
            OnFailureInput = input;

            return this;
        }

        public EntryConnector ToEntryConnector()
        {
            return new EntryConnector
            {
                ConnectorKey = ConnectorKey,
                Description = Description,
                Configuration = _settings,
                FailureTransition = OnFailureInput != null 
                    ? new ExceptionTransition { Input = OnFailureInput } 
                    : null
            };
        }
    }
}