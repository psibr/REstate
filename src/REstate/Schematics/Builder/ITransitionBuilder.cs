using System;

namespace REstate.Schematics.Builder
{
    public interface ITransitionBuilder<TState, TInput>
        : ITransition<TState, TInput>
    {
        ITransitionBuilder<TState, TInput> WithGuard(ConnectorKey connectorKey, Action<IGuardBuilder> guard = null);
    }
}