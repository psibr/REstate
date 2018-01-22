using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using REstate.Tests.Features.Context;

namespace REstate.Engine.Repositories.EntityFrameworkCore.Tests.Features.Context
{
    public class REstateEntityFrameworkCoreContext<TState, TInput>
        : REstateContext<TState, TInput>
    {
        #region GIVEN
        public Task Given_EntityFrameworkCore_is_the_registered_repository()
        {
            CurrentHost.Agent().Configuration
                .RegisterComponent(new EntityFrameworkCoreRepositoryComponent(builder => builder.UseInMemoryDatabase("REstateScenarioTests")));

            return Task.CompletedTask;
        }
        #endregion

        #region WHEN

        #endregion

        #region THEN

        #endregion
    }
}
