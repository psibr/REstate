using System;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    public class RedisRepositoryContextFactory<TState, TInput>
        : IRepositoryContextFactory<TState, TInput>
    {
        private readonly IDatabaseAsync _restateDatabase;

        public RedisRepositoryContextFactory(IDatabaseAsync restateDatabase)
        {
            _restateDatabase = restateDatabase;
        }

        public Task<IEngineRepositoryContext<TState, TInput>> OpenContextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult<IEngineRepositoryContext<TState, TInput>>(new RedisEngineRepositoryContext<TState, TInput>(_restateDatabase));
        }
    }
}
