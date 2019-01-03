using Microsoft.EntityFrameworkCore;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public class REstateDbContext
        : DbContext
    {
        internal REstateDbContext(DbContextOptions options)
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

            modelBuilder.Entity<Machine>()
                .HasKey(status => status.MachineId);
            
            modelBuilder.Entity<Machine>()
                .Property(machine => machine.MachineId)
                .IsUnicode(false)
                .HasMaxLength(450)
                .IsRequired();

            modelBuilder.Entity<Machine>()
                .Property(machine => machine.StateJson)
                .IsUnicode(false)
                .IsRequired();
            
            modelBuilder.Entity<Machine>()
                .Property(status => status.CommitNumber)
                .IsRequired()
                .IsConcurrencyToken();

            modelBuilder.Entity<Machine>()
                .Property(machine => machine.SchematicName)
                .IsUnicode(false)
                .HasMaxLength(450)
                .IsRequired(false);

            modelBuilder.Entity<Machine>()
                .Property(status => status.UpdatedTime)
                .IsRequired();

            modelBuilder.Entity<Machine>()
                .HasMany(machine => machine.MetadataEntries)
                .WithOne();
            
            modelBuilder.Entity<Machine>()
                .HasMany(machine => machine.StateBagEntries)
                .WithOne();

            modelBuilder.Entity<MetadataEntry>()
                .HasKey(entry => new
                {
                    entry.MachineId,
                    entry.Key
                });
            
            modelBuilder.Entity<MetadataEntry>()
                .Property(entry => entry.MachineId)
                .IsUnicode(false)
                .HasMaxLength(450)
                .IsRequired();

            modelBuilder.Entity<MetadataEntry>()
                .Property(entry => entry.Key)
                .IsUnicode(false)
                .HasMaxLength(450)
                .IsRequired();

            modelBuilder.Entity<StateBagEntry>()
                .HasKey(entry => new
                {
                    entry.MachineId,
                    entry.Key
                });
            
            modelBuilder.Entity<StateBagEntry>()
                .Property(entry => entry.MachineId)
                .IsUnicode(false)
                .HasMaxLength(450)
                .IsRequired();
            
            modelBuilder.Entity<StateBagEntry>()
                .Property(entry => entry.Key)
                .IsUnicode(false)
                .HasMaxLength(450)
                .IsRequired();

            modelBuilder.Entity<Schematic>()
                .HasKey(schematic => schematic.SchematicName);

            modelBuilder.Entity<Schematic>()
                .Property(schematic => schematic.SchematicName)
                .IsUnicode(false)
                .IsRequired();
        }

        public DbSet<Machine> Machines { get; set; }

        public DbSet<MetadataEntry> MetadataEntries { get; set; }
        
        public DbSet<StateBagEntry> StateBagEntries { get; set; }

        public DbSet<Schematic> Schematics { get; set; }
    }
}