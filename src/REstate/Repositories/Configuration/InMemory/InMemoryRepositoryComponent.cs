using REstate.IoC;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryRepositoryComponent
        : IComponent
    {
        public void Register(IRegistrar registrar)
        {
            registrar.Register<IRepositoryContextFactory>(c =>
                new InMemoryRepositoryContextFactory(
                    c.Resolve<StringSerializer>()));
        }
    }
}
