using System;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    public class RedisEngineRepositoryContext<TState, TInput>
        : IEngineRepositoryContext<TState, TInput>
    {

        public RedisEngineRepositoryContext(IDatabaseAsync redisDatabase)
        {
            // No need to seperate the implementations, so we use a shared repo.
            var repo = new RedisEngineRepository<TState, TInput>(redisDatabase);

            Schematics = repo;
            Machines = repo;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Multiplexer perfers re-use, so we won't dispose of it.
        }

        public ISchematicRepository<TState, TInput> Schematics { get; }

        public IMachineRepository<TState, TInput> Machines { get; }
    }
}
