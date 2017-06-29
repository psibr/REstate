using System;
using System.Collections.Generic;
using System.Globalization;

namespace REstate.Configuration.Builder
{
    public interface IEntryActionBuilder 
        : IEntryAction
    {
        IEntryActionBuilder DescribedAs(string description);

        IEntryActionBuilder WithSetting(string key, string value);
        IEntryActionBuilder WithSetting(KeyValuePair<string, string> setting);
        IEntryActionBuilder WithSetting(ValueTuple<string, string> setting);

        IEntryActionBuilder OnFailureSend(Input input);
    }

    public interface IEntryAction
    {
        string ConnectorKey { get; }

        string Description { get; }

        string OnFailureInput { get; }

        IReadOnlyDictionary<string, string> Settings { get; }

        EntryConnector ToEntryConnector();
    }
}