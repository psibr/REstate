namespace REstate.Engine.Connectors
{
    public class InputParameters<TInput, TPayload> 
        : IInputParameters<TInput, TPayload>
    {
        public InputParameters(TInput input, TPayload payload)
        {
            Input = input;
            Payload = payload;
        }

        public TInput Input { get; }

        public TPayload Payload { get; }
    }
}
