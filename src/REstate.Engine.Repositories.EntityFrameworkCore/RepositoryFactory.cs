using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{

    public class RepositoryFactory<TState, TInput>
        : IRepositoryContextFactory<TState, TInput>
    {
        private readonly REstateDbContextFactory _dbContextFactory;

        internal RepositoryFactory(REstateDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public Task<IEngineRepositoryContext<TState, TInput>> OpenContextAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEngineRepositoryContext<TState, TInput>>(
                new Repository<TState, TInput>(_dbContextFactory.CreateContext()));
        }
    }

}
