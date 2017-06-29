using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public interface IGuardBuilder
        : IGuard
    {
        IGuardBuilder DescribedAs(string description);

        IGuardBuilder WithSetting(string key, string value);
        IGuardBuilder WithSetting(KeyValuePair<string, string> setting);
        IGuardBuilder WithSetting(ValueTuple<string, string> setting);
    }

    public interface IGuard
    {
        string ConnectorKey { get; }

        string Description { get; }

        IReadOnlyDictionary<string, string> Settings { get; }

        GuardConnector ToGuardConnector();
    }
}