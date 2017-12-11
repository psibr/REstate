using System.Collections.Generic;

namespace REstate.Schematics
{
    public interface IEntryAction<TInput>
    {
        ConnectorKey ConnectorKey { get; }

        string Description { get; }
        
        IExceptionInput<TInput> OnExceptionInput { get; }

        IReadOnlyDictionary<string, string> Settings { get; }
    }
}