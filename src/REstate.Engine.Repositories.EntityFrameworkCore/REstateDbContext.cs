using System;
using Microsoft.EntityFrameworkCore;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public class EntityFrameworkCoreMachineStatus
    {
        public string MachineId { get; set; }
        
        public long CommitNumber { get; set; }

        public DateTimeOffset UpdatedTime { get; set; }

        public string StateJson { get; set; }

        public string MetadataJson { get; set; }

        public string SchematicJson { get; set; }
    }

    public class EntityFrameworkCoreSchematic
    {
        public string SchematicName { get; set; }

        public string SchematicJson { get; set; }
    }

    public class REstateDbContext
        : DbContext
    {
        public REstateDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        ///     Override this method to further configure the model that was discovered by convention from the entity types
        ///     exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        ///     and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <remarks>
        ///     If a model is explicitly set on the options for this context 
        ///     (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        ///     then this method will not be run.
        /// </remarks>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model for this context. Databases (and other extensions) typically
        ///     define extension methods on this object that allow you to configure aspects of the model that are specific
        ///     to a given database.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EntityFrameworkCoreMachineStatus>()
                .HasKey(status => status.MachineId);

            modelBuilder.Entity<EntityFrameworkCoreMachineStatus>()
                .Property(status => status.CommitNumber)
                .IsConcurrencyToken();

            modelBuilder.Entity<EntityFrameworkCoreSchematic>()
                .HasKey(schematic => schematic.SchematicName);
        }

        public DbSet<EntityFrameworkCoreMachineStatus> Machines { get; set; }

        public DbSet<EntityFrameworkCoreSchematic> Schematics { get; set; }
    }
}
