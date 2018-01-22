using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{

    public class EntityFrameworkCoreRepositoryContextFactory<TState, TInput>
        : IRepositoryContextFactory<TState, TInput>
    {
        internal EntityFrameworkCoreRepositoryContextFactory(REstateEntityFrameworkCoreServerOptions options)
        {
            Options = options;
        }

        private REstateEntityFrameworkCoreServerOptions Options { get; }

        public Task<IEngineRepositoryContext<TState, TInput>> OpenContextAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEngineRepositoryContext<TState, TInput>>(
                new EntityFrameworkCoreEngineRepositoryContext<TState, TInput>(
                    new REstateDbContext(Options.DbContextOptions)));
        }
    }

}
