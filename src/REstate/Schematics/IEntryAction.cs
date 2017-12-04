using System.Collections.Generic;

namespace REstate.Schematics
{
    public interface IEntryAction<TInput>
    {
        ConnectorKey ConnectorKey { get; }

        string Description { get; }

        TInput OnFailureInput { get; }

        IReadOnlyDictionary<string, string> Settings { get; }
    }
}