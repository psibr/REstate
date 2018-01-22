using Microsoft.EntityFrameworkCore;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    internal class REstateEntityFrameworkCoreServerOptions
    {
        public REstateEntityFrameworkCoreServerOptions(DbContextOptions dbContextOptions)
        {
            DbContextOptions = dbContextOptions;
        }

        public DbContextOptions DbContextOptions { get; }
    }

}
