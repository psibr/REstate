using System;
using System.Threading.Tasks;
using REstate.IoC;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    /// <summary>
    /// REstate component that configures Redis as the state storage system.
    /// </summary>
    public class RedisRepositoryComponent
        : IComponent
    {
        private readonly IDatabase _restateDatabase;

        /// <summary>
        /// Creates an instance of the component given an <see cref="StackExchange.Redis.IDatabase"/>
        /// </summary>
        /// <param name="restateDatabase">The database REstate will use on a Redis server</param>
        public RedisRepositoryComponent(IDatabase restateDatabase)
        {
            _restateDatabase = restateDatabase;
        }

        /// <summary>
        /// Registers the dependencies of this component and actives necessary configuration
        /// </summary>
        /// <param name="registrar">The registrar to which the configuration and dependencies should be added</param>
        public void Register(IRegistrar registrar)
        {
            registrar.Register(new REstateRedisDatabase(_restateDatabase));
            registrar.Register(typeof(IRepositoryContextFactory<,>), typeof(RedisRepositoryContextFactory<,>));
        }
    }
}
