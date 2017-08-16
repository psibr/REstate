using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    public class REstateRedisDatabase
    {
        public REstateRedisDatabase(IDatabase redisDatabase)
        {
            RedisDatabase = redisDatabase;
        }

        public IDatabase RedisDatabase { get; }
    }
}