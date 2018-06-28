namespace REstate.Engine.Connectors
{
    public interface IInputParameters<TInput, out TPayload>
    {
        TInput Input { get; }

        TPayload Payload { get; }
    }
}