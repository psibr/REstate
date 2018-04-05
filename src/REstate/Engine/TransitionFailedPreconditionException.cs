using System;
using REstate.Engine.Connectors;
using REstate.Schematics;

namespace REstate.Engine
{
    public class TransitionFailedPreconditionException<TState, TInput, TPayload>
        : Exception
    {
        public Status<TState> FromState { get; }
        public ITransition<TState, TInput> FailedTransition { get; }
        public InputParameters<TInput, TPayload> InputParameters { get; }

        public TransitionFailedPreconditionException(Status<TState> fromState,
            ITransition<TState, TInput> transition,
            InputParameters<TInput, TPayload> inputParameters)
            : this(
                  fromState, 
                  transition, 
                  inputParameters, 
                  $"Transition from  {transition.Input} to {transition.ResultantState} failed while validating precondition")
        { }

        public TransitionFailedPreconditionException(Status<TState> fromState,
            ITransition<TState, TInput> transition,
            InputParameters<TInput, TPayload> inputParameters,
            string message)
            : this(
                  fromState,
                  transition,
                  inputParameters,
                  message,
                  default)
        { }

        public TransitionFailedPreconditionException(Status<TState> fromState,
            ITransition<TState, TInput> transition,
            InputParameters<TInput, TPayload> inputParameters,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            FromState = fromState;
            FailedTransition = transition;
            InputParameters = inputParameters;
        }
    }
}
