using System;
using System.Threading.Tasks;
using REstate.IoC;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    public class RedisRepositoryComponent
        : IComponent
    {
        private readonly  IDatabaseAsync _restateDatabase;

        public RedisRepositoryComponent(IDatabaseAsync restateDatabase)
        {
            _restateDatabase = restateDatabase;
        }

        public void Register(IRegistrar registrar)
        {
            registrar.Register(_restateDatabase);
            registrar.Register(typeof(IRepositoryContextFactory<,>), typeof(RedisRepositoryContextFactory<,>));
        }
    }
}
