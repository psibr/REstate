namespace REstate.Engine.Services
{
    public class InputParameters<TInput, TPayload>
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