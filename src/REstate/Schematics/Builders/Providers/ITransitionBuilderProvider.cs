namespace REstate.Schematics.Builders.Providers
{
    public interface ITransitionBuilderProvider<TState, TInput, out TThis>
        : ITransition<TState, TInput>
    {
        TThis WithPrecondition(ConnectorKey connectorKey, System.Action<IPreconditionBuilder> precondition = null);
    }
}
