using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    /// <summary>
    /// An adapter for the <see cref="StackExchange.Redis.IDatabase" /> that REstate will use for state tracking
    /// </summary>
    internal class REstateRedisDatabase
    {
        public REstateRedisDatabase(IDatabase redisDatabase)
        {
            RedisDatabase = redisDatabase;
        }

        public IDatabase RedisDatabase { get; }
    }
}
