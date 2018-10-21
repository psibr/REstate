using System;
using Microsoft.EntityFrameworkCore;
using REstate.IoC;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    /// <summary>
    /// REstate component that configures Entity Framework Core as the state storage system.
    /// </summary>
    public class EntityFrameworkCoreRepositoryComponent
        : IComponent
    {
        private readonly DbContextOptions _dbContextOptions;

        /// <summary>
        /// Creates an instance of the component given configuration using a <see cref="DbContextOptionsBuilder"/>. />
        /// </summary>
        /// <param name="dbContextOptions">The connection options REstate will use</param>
        public EntityFrameworkCoreRepositoryComponent(DbContextOptions dbContextOptions)
        {
            _dbContextOptions = dbContextOptions;
        }

        /// <summary>
        /// Registers the dependencies of this component and actives necessary configuration
        /// </summary>
        /// <param name="registrar">The registrar to which the configuration and dependencies should be added</param>
        public void Register(IRegistrar registrar)
        {
            registrar.Register(new REstateDbContextFactory(_dbContextOptions));
            registrar.Register(typeof(IRepositoryContextFactory<,>), typeof(RepositoryFactory<,>));
        }
    }

}
