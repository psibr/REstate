using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    internal class RedisRepositoryContextFactory<TState, TInput>
        : IRepositoryContextFactory<TState, TInput>
    {
        private readonly IDatabase _restateDatabase;

        public RedisRepositoryContextFactory(REstateRedisDatabase restateDatabase)
        {
            _restateDatabase = restateDatabase.RedisDatabase;
        }

        public IMachineStatusStore<TState, TInput> GetMachineStatusStore(string machineId)
        {
            return new RedisMachineStatusStore<TState, TInput>(_restateDatabase, machineId);
        }

        public Task<IEngineRepositoryContext<TState, TInput>> OpenContextAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEngineRepositoryContext<TState, TInput>>(
                new RedisEngineRepositoryContext<TState, TInput>(_restateDatabase));
        }
    }
}
