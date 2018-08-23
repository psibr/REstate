using System;
using System.Collections.Generic;
using REstate.Schematics.Builders.Providers;

namespace REstate.Schematics.Builders
{
    public interface IPreconditionBuilder
        : IPreconditionBuilderProvider<IPreconditionBuilder>
    {

    }

    internal class PreconditionBuilder 
        : IPreconditionBuilder
    {
        public PreconditionBuilder(ConnectorKey connectorKey)
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

        public IPreconditionBuilder DescribedAs(string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description, if provided, cannot be empty or whitespace.", nameof(description));

            Description = description;

            return this;
        }

        public IPreconditionBuilder WithSetting(string key, string value)
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

        public IPreconditionBuilder WithSetting(KeyValuePair<string, string> setting) =>
            WithSetting(setting.Key, setting.Value);

        public IPreconditionBuilder WithSetting((string key, string value) setting) =>
            WithSetting(setting.key, setting.value);
    }
}
