using System;

namespace REstate.Schematics.Builder
{
    public interface ITransitionBuilder<TState, TInput>
        : ITransition<TState, TInput>
    {
        ITransitionBuilder<TState, TInput> WithGuard(string connectorKey, Action<IGuardBuilder> guard = null);
    }
}