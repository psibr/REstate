using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace REstate.Engine.Repositories.EntityFrameworkCore.Tests
{
    /// <summary>
    /// Tests providers to ensure they support required features for REstate.
    /// </summary>
    public class ProviderValidationTests
    {
        public static object[][] Providers = 
        {
            new object[] { new Provider("InMemory", builder => builder.UseInMemoryDatabase("InMemory")) },
            
            // Uncomment below to run provider specific tests that require external databases
            
            new object[] { new Provider("SqlServer", builder => builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=REstateEfTests;Trusted_Connection=True;MultipleActiveResultSets=true")) }

        };

        [Theory]
        [MemberData(nameof(Providers))]
        public async Task ConcurrencyCheckIsSupported(Provider provider)
        {
            // Arrange
            var contextFactory = new SampleDbContextFactory(provider.Action);

            var id = Guid.NewGuid();

            using (var dbContext = contextFactory.CreateContext())
            {
                await dbContext.Database.EnsureDeletedAsync();
                await dbContext.Database.EnsureCreatedAsync();

                dbContext.Data.Add(new Datum
                {
                    Id = id,
                    Counter = 0,
                    CommitTag = Guid.NewGuid()
                });

                await dbContext.SaveChangesAsync();
            }

            async Task ModifyDataAsync()
            {
                while (true)
                {
                    using var dbContext = contextFactory.CreateContext();
                    var entity = await dbContext.Data.SingleAsync(datum => datum.Id == id);

                    entity.Counter++;
                    entity.CommitTag = Guid.NewGuid();

                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        continue;
                    }

                    break;
                }
            }

            // Act
            var t1 = Task.Run(async () => await ModifyDataAsync());
            var t2 = Task.Run(async () => await ModifyDataAsync());
            var t3 = Task.Run(async () => await ModifyDataAsync());
            var t4 = Task.Run(async () => await ModifyDataAsync());

            await Task.WhenAll(t1, t2, t3, t4);

            Datum result;
            using (var dbContext = contextFactory.CreateContext())
            {
                result = await dbContext.Data.SingleAsync(datum => datum.Id == id);
            }

            // Assert

            Assert.Equal(4, result.Counter);
        }

        public class SampleDbContextFactory
        {
            private DbContextOptions Options { get; }

            public SampleDbContextFactory(Action<DbContextOptionsBuilder> providerAction)
            {
                var builder = new DbContextOptionsBuilder();

                providerAction(builder);

                Options = builder.Options;
            }

            public SampleDbContext CreateContext()
            {
                return new SampleDbContext(Options);
            }
        }

        public class SampleDbContext
            : DbContext
        {
            public SampleDbContext(DbContextOptions options)
            : base(options)
            {
                
            }

            public DbSet<Datum> Data { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Datum>()
                    .HasKey(datum => datum.Id);

                modelBuilder.Entity<Datum>()
                    .Property(datum => datum.CommitTag)
                    .IsConcurrencyToken();
            }
        }

        public class Datum
        {
            public Guid Id { get; set; }

            public int Counter { get; set; }

            public Guid CommitTag { get; set; }
        }

        public class Provider
        {
            public Provider(string name, Action<DbContextOptionsBuilder> action)
            {
                Name = name;
                Action = action;
            }

            public string Name { get; }

            public Action<DbContextOptionsBuilder> Action { get; }

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString()
            {
                return Name;
            }
        }
    }
}
