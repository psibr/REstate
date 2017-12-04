using System.Collections.Generic;

namespace REstate.Schematics
{
    public interface IGuard
    {
        ConnectorKey ConnectorKey { get; }

        string Description { get; }

        IReadOnlyDictionary<string, string> Settings { get; }
    }
}