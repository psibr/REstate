using System;
using System.Collections.Generic;

namespace REstate.Schematics.Builder.Implementation
{
    internal class GuardBuilder 
        : IGuardBuilder
    {
        public GuardBuilder(ConnectorKey connectorKey)
        {
            if (connectorKey.Identifier == null)
                throw new ArgumentNullException(nameof(connectorKey), "ConnectorKey.Identifier cannot be null.");
            if (string.IsNullOrWhiteSpace(connectorKey.Identifier))
                throw new ArgumentException("ConnectorKey.Identifier cannot be empty or whitespace.", nameof(connectorKey));

            ConnectorKey = connectorKey;
        }

        public ConnectorKey ConnectorKey { get; }

        public string Description { get; private set; }

        public IReadOnlyDictionary<string, string> Settings => _settings;

        private readonly Dictionary<string, string> _settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IGuardBuilder DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description, if provided, cannot be empty or whitespace.", nameof(description));

            Description = description;

            return this;
        }

        public IGuardBuilder WithSetting(string key, string value)
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

        public IGuardBuilder WithSetting(KeyValuePair<string, string> setting) =>
            WithSetting(setting.Key, setting.Value);

        public IGuardBuilder WithSetting((string, string) setting) =>
            WithSetting(setting.Item1, setting.Item2);
    }
}