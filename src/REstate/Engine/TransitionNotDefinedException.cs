using System;

namespace REstate.Engine
{
    public class TransitionNotDefinedException<TState, TInput>
        : Exception
    {
        public Status<TState> CurrentStatus { get; }
        public TInput Input { get; }

        public TransitionNotDefinedException(Status<TState> currentStatus,
            TInput input)
            : this(currentStatus,
                  input,
                $"No transition defined for status: '{currentStatus.State}' using input: '{input}'")
        { }

        public TransitionNotDefinedException(Status<TState> currentStatus,
            TInput input,
            string message)
            : this(currentStatus,
                  input,
                  message,
                  default)
        { }

        public TransitionNotDefinedException(Status<TState> currentStatus,
            TInput input,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            CurrentStatus = currentStatus;
            Input = input;
        }
    }
}
