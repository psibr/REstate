using System;
using System.Collections.Generic;

namespace REstate.Schematics.Builder
{
    public interface IEntryActionBuilder<TInput> 
        : IEntryAction<TInput>
    {
        IEntryActionBuilder<TInput> DescribedAs(string description);

        IEntryActionBuilder<TInput> WithSetting(string key, string value);
        IEntryActionBuilder<TInput> WithSetting(KeyValuePair<string, string> setting);
        IEntryActionBuilder<TInput> WithSetting(ValueTuple<string, string> setting);

        IEntryActionBuilder<TInput> OnFailureSend(TInput input);
    }
}