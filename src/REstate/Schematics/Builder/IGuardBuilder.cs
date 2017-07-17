using System;
using System.Collections.Generic;

namespace REstate.Schematics.Builder
{
    public interface IGuardBuilder
        : IGuard
    {
        IGuardBuilder DescribedAs(string description);

        IGuardBuilder WithSetting(string key, string value);
        IGuardBuilder WithSetting(KeyValuePair<string, string> setting);
        IGuardBuilder WithSetting(ValueTuple<string, string> setting);
    }
}