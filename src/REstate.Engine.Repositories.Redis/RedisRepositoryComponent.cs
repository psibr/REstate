using System;
using System.Threading.Tasks;
using REstate.IoC;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    public class RedisRepositoryComponent
        : IComponent
    {
        private readonly IDatabase _restateDatabase;

        public RedisRepositoryComponent(IDatabase restateDatabase)
        {
            _restateDatabase = restateDatabase;
        }

        public void Register(IRegistrar registrar)
        {
            registrar.Register(new REstateRedisDatabase(_restateDatabase));
            registrar.Register(typeof(IRepositoryContextFactory<,>), typeof(RedisRepositoryContextFactory<,>));
        }
    }
}
