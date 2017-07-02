namespace REstate.Engine.Repositories
{
    public interface IRepositoryContextFactory<TState>
    {
        IEngineRepositoryContext<TState> OpenContext();
    }
}
