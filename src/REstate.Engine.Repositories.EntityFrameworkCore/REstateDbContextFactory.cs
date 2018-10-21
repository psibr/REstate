using Microsoft.EntityFrameworkCore;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public class REstateDbContextFactory
    {
        private readonly DbContextOptions _options;

        public REstateDbContextFactory(DbContextOptions options)
        {
            _options = options;
        }

        public REstateDbContext CreateContext()
        {
            return new REstateDbContext(_options);
        }
    }
}