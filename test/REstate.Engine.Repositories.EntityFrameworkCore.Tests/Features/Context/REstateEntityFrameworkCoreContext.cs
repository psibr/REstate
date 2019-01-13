using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using REstate.Tests.Features.Context;

namespace REstate.Engine.Repositories.EntityFrameworkCore.Tests.Features.Context
{
    public class REstateEntityFrameworkCoreContext<TState, TInput>
        : REstateContext<TState, TInput>
    {
        #region GIVEN
        public async Task Given_EntityFrameworkCore_is_the_registered_repository()
        {
            var options = new DbContextOptionsBuilder()
                //.UseInMemoryDatabase("REstateScenarioTests")
                .UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=REstateEfTestsIntegrated;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;

            CurrentHost.Agent().Configuration
                .RegisterComponent(new EntityFrameworkCoreRepositoryComponent(
                    options));

            await new REstateDbContextFactory(options).CreateContext().Database.EnsureCreatedAsync(CancellationToken.None);

            //return Task.CompletedTask;
        }
        #endregion

        #region WHEN

        #endregion

        #region THEN

        #endregion
    }
}
