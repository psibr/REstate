using System;

namespace REstate.Configuration.Builder.Implementation
{
    internal class TransitionBuilder<TState>
        : ITransitionBuilder<TState>
    {
        public TransitionBuilder(Input input, TState resultantState)
        {
            Input = input;
            ResultantState = resultantState;
        }

        public Input Input { get; }
        public TState ResultantState { get; }
        public IGuard Guard { get; private set; }

        public ITransitionBuilder<TState> WithGuard(string connectorKey, Action<IGuardBuilder> guard = null)
        {
            var guardBuilder = new GuardBuilder(connectorKey);

            guard?.Invoke(guardBuilder);

            Guard = guardBuilder;

            return this;
        }
        public Transition<TState> ToTransition()
        {
            return new Transition<TState>
            {
                InputName = Input,
                ResultantState = ResultantState,
                Guard = Guard?.ToGuardConnector()
            };
        }
    }
}