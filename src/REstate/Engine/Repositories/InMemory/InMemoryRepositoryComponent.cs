using REstate.IoC;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryRepositoryComponent
        : IComponent
    {
        public void Register(IRegistrar registrar)
        {
            registrar.Register(typeof(IRepositoryContextFactory<>), typeof(InMemoryRepositoryContextFactory<>));
        }
    }
}
