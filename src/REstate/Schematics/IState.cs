using System.Collections.Generic;

namespace REstate.Schematics
{
    public interface IState<TState, TInput>
    {
        TState Value { get; }

        TState ParentState { get; }

        string Description { get; }

        IDictionary<TInput, ITransition<TState, TInput>> Transitions { get; }

        IEntryAction<TInput> OnEntry { get; }
    }
}