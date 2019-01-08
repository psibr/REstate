using System.Collections.Generic;

namespace REstate.Schematics
{
    public interface IAction<TInput>
    {
        ConnectorKey ConnectorKey { get; }

        string Description { get; }
        
        IExceptionInput<TInput> OnExceptionInput { get; }

        IReadOnlyDictionary<string, string> Settings { get; }

        bool LongRunning { get; }
    }
}