using System;

namespace REstate.Configuration.Builder.Implementation
{
    internal class TransitionBuilder 
        : ITransitionBuilder
    {
        public TransitionBuilder(Input input, string resultantStateName)
        {
            if (resultantStateName == null)
                throw new ArgumentNullException(nameof(resultantStateName));
            if (string.IsNullOrWhiteSpace(resultantStateName))
                throw new ArgumentException("Value cannot be empty or whitespace.", nameof(resultantStateName));

            Input = input;
            ResultantStateName = resultantStateName;
        }

        public Input Input { get; }
        public string ResultantStateName { get; }
        public IGuard Guard { get; private set; }

        public ITransitionBuilder WithGuard(string connectorKey, Action<IGuardBuilder> guard = null)
        {
            var guardBuilder = new GuardBuilder(connectorKey);

            guard?.Invoke(guardBuilder);

            Guard = guardBuilder;

            return this;
        }
        public Transition ToTransition()
        {
            return new Transition
            {
                InputName = Input,
                ResultantStateName = ResultantStateName,
                Guard = Guard?.ToGuardConnector()
            };
        }
    }
}