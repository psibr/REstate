using System;
using System.Collections.Generic;

namespace REstate.Schematics.Builder.Providers
{
    public interface IPreconditionBuilderProvider<out TThis>
        : IPrecondition
    {
        TThis DescribedAs(string description);

        TThis WithSetting(string key, string value);
        TThis WithSetting(KeyValuePair<string, string> setting);
        TThis WithSetting((string key, string value) setting);
    }
}
