using System.Threading;
using System.Threading.Tasks;
using REstate.Tests.Features.Context;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis.Tests.Features.Context
{
    public class REstateRedisContext<TState, TInput>
        : REstateContext<TState, TInput>
    {
        #region GIVEN
        public Task Given_Redis_is_the_registered_repository()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
            var redisDatabase = redis.GetDatabase();

            CurrentHost.Agent().Configuration
                .RegisterComponent(new RedisRepositoryComponent(
                    redisDatabase));

            return Task.CompletedTask;
        }
        #endregion

        #region WHEN

        #endregion

        #region THEN

        #endregion
    }
}
