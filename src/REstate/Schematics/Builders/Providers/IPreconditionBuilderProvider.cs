using System.Collections.Generic;

namespace REstate.Schematics.Builders.Providers
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
