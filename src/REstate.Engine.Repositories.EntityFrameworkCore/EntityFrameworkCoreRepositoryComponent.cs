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
        private readonly REstateEntityFrameworkCoreServerOptions _options;

        /// <summary>
        /// Creates an instance of the component given configurationusing a <see cref="DbContextOptionsBuilder"/>. />
        /// </summary>
        /// <param name="optionsBuilder">The connection options REstate will use</param>
        public EntityFrameworkCoreRepositoryComponent(Action<DbContextOptionsBuilder> optionsBuilder)
        {
            var builder = new DbContextOptionsBuilder();

            optionsBuilder?.Invoke(builder);

            _options = new REstateEntityFrameworkCoreServerOptions(builder.Options);
        }

        /// <summary>
        /// Registers the dependencies of this component and actives necessary configuration
        /// </summary>
        /// <param name="registrar">The registrar to which the configuration and dependencies should be added</param>
        public void Register(IRegistrar registrar)
        {
            registrar.Register(_options);
            registrar.Register(typeof(IRepositoryContextFactory<,>), typeof(EntityFrameworkCoreRepositoryContextFactory<,>));
        }
    }

}
