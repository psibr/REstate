using System;

namespace REstate.Engine
{
    public class TransitionNotDefinedException
        : Exception
    {
        public TransitionNotDefinedException(string state, string input)
            : base($"No transition defined for state {state} with input {input}.")
        {
        }

        public TransitionNotDefinedException(string message)
            : base(message)
        {
        }

        public TransitionNotDefinedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
