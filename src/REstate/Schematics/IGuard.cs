using System.Collections.Generic;

namespace REstate.Schematics
{
    public interface IPrecondition
    {
        ConnectorKey ConnectorKey { get; }

        string Description { get; }

        IReadOnlyDictionary<string, string> Settings { get; }
    }
}