using System;

namespace REstate.Configuration.Builder
{
    public interface ITransitionBuilder
        : ITransition
    {
        ITransitionBuilder WithGuard(string connectorKey, Action<IGuardBuilder> guard = null);
    }

    public interface ITransition
    {
        Input Input { get; }

        string ResultantStateName { get; }

        IGuard Guard { get; }

        Transition ToTransition();
    }
}