namespace REstate.Engine
{
    /// <summary>
    /// A <see cref="IStateEngine&lt;TState, TInput&gt;" /> that executes state changes locally
    /// </summary>
    public interface ILocalStateEngine<TState, TInput>
        : IStateEngine<TState, TInput>
    {
    }
}
