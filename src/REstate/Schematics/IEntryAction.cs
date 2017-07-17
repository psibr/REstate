using System.Collections.Generic;

namespace REstate.Schematics
{
    public interface IEntryAction<TInput>
    {
        string ConnectorKey { get; }

        string Description { get; }

        TInput OnFailureInput { get; }

        IReadOnlyDictionary<string, string> Settings { get; }
    }
}