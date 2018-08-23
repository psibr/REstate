using REstate.Schematics.Builders.Providers;

namespace REstate.Schematics.Builders
{
    public interface ITransitionBuilder<TState, TInput>
        : ITransitionBuilderProvider<TState, TInput, ITransitionBuilder<TState, TInput>>
    {

    }

    internal class TransitionBuilder<TState, TInput>
        : ITransitionBuilder<TState, TInput>
    {
        public TransitionBuilder(TInput input, TState resultantState)
        {
            Input = input;
            ResultantState = resultantState;
        }

        public TInput Input { get; }
        public TState ResultantState { get; }
        public IPrecondition Precondition { get; private set; }

        public ITransitionBuilder<TState, TInput> WithPrecondition(
            ConnectorKey connectorKey,
            System.Action<IPreconditionBuilder> guard = null)
        {
            var preconditionBuilder = new PreconditionBuilder(connectorKey);

            guard?.Invoke(preconditionBuilder);

            Precondition = preconditionBuilder;

            return this;
        }
    }
}
