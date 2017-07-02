using System;

namespace REstate.Configuration.Builder
{
    public interface ITransitionBuilder<TState>
        : ITransition<TState>
    {
        ITransitionBuilder<TState> WithGuard(string connectorKey, Action<IGuardBuilder> guard = null);
    }

    public interface ITransition<TState>
    {
        Input Input { get; }

        TState ResultantState { get; }

        IGuard Guard { get; }

        Transition<TState> ToTransition();
    }
}