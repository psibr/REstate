using System;

namespace REstate.Configuration.Builder
{
    public interface ITransitionBuilder<TState, TInput>
        : ITransition<TState, TInput>
    {
        ITransitionBuilder<TState, TInput> WithGuard(string connectorKey, Action<IGuardBuilder> guard = null);
    }

    public interface ITransition<TState, TInput>
    {
        TInput Input { get; }

        TState ResultantState { get; }

        IGuard Guard { get; }

        Transition<TState, TInput> ToTransition();
    }
}