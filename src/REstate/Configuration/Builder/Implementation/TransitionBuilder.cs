using System;

namespace REstate.Configuration.Builder.Implementation
{
    internal class TransitionBuilder<TState, TInput>
        : ITransitionBuilder<TState, TInput>
    {
        public TransitionBuilder(TInput input, TState resultantState)
        {
            Input = input;
            ResultantState = resultantState;
        }

        public TInput Input { get; }
        public TState ResultantState { get; }
        public IGuard Guard { get; private set; }

        public ITransitionBuilder<TState, TInput> WithGuard(string connectorKey, Action<IGuardBuilder> guard = null)
        {
            var guardBuilder = new GuardBuilder(connectorKey);

            guard?.Invoke(guardBuilder);

            Guard = guardBuilder;

            return this;
        }
        public Transition<TState, TInput> ToTransition()
        {
            return new Transition<TState, TInput>
            {
                Input = Input,
                ResultantState = ResultantState,
                Guard = Guard?.ToGuardConnector()
            };
        }
    }
}