using System;
using System.Collections.Generic;

namespace REstate.Schematics.Builder.Implementation
{
    internal class EntryActionBuilder<TInput> 
        : IEntryActionBuilder<TInput>
    {
        public EntryActionBuilder(ConnectorKey connectorKey)
        {
            if (connectorKey == null)
                throw new ArgumentNullException(nameof(connectorKey));
            if (connectorKey.Identifier == null)
                throw new ArgumentNullException(nameof(connectorKey.Identifier));
            if (string.IsNullOrWhiteSpace(connectorKey.Identifier))
                throw new ArgumentException("Value cannot be empty or whitespace.", nameof(connectorKey));

            ConnectorKey = connectorKey;
        }

        public ConnectorKey ConnectorKey { get; }
        public string Description { get; private set; }
        public IExceptionInput<TInput> OnExceptionInput { get; private set; }

        public IReadOnlyDictionary<string, string> Settings => _settings;

        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IEntryActionBuilder<TInput> DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description, if provided, cannot be empty or whitespace.", nameof(description));

            Description = description;

            return this;
        }

        public IEntryActionBuilder<TInput> WithSetting(string key, string value)
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

        public IEntryActionBuilder<TInput> WithSetting(KeyValuePair<string, string> setting) => 
            WithSetting(setting.Key, setting.Value);

        public IEntryActionBuilder<TInput> WithSetting((string, string) setting) => 
            WithSetting(setting.Item1, setting.Item2);

        public IEntryActionBuilder<TInput> OnFailureSend(TInput input)
        {
            OnExceptionInput = new ExceptionInput<TInput>(input);

            return this;
        }
    }
}