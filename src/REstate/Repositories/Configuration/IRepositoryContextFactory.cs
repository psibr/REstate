namespace REstate.Engine.Repositories
{
    public interface IRepositoryContextFactory
    {
        IEngineRepositoryContext OpenContext();
    }
}
