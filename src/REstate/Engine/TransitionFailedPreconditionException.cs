using System;

namespace REstate.Engine
{
    public class TransitionFailedPreconditionException
        : Exception
    {
        public TransitionFailedPreconditionException(string state, string input, string resultantState)
            : this($"Transition from state {state} with input {input} to state {resultantState} failed while validating precondition.")
        { }

        public TransitionFailedPreconditionException(string message)
            : base(message)
        { }

        public TransitionFailedPreconditionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
