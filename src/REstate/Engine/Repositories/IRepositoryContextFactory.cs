namespace REstate.Engine.Repositories
{
    public interface IRepositoryContextFactory<TState, TInput>
    {
        IEngineRepositoryContext<TState, TInput> OpenContext();
    }
}
