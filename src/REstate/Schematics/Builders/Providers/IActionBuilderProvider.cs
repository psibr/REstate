using System;
using System.Collections.Generic;

namespace REstate.Schematics.Builders.Providers
{
    public interface IActionBuilderProvider<TInput, out TThis>
        : IAction<TInput>
    {
        TThis DescribedAs(string description);

        TThis WithSetting(string key, string value);
        TThis WithSetting(KeyValuePair<string, string> setting);
        TThis WithSetting(ValueTuple<string, string> setting);

        TThis OnFailureSend(TInput input);

        TThis IsLongRunning();
    }
}
