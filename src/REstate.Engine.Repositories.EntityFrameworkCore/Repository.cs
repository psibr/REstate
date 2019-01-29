using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using REstate.Schematics;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public class Repository<TState, TInput>
        : IEngineRepositoryContext<TState, TInput>
        , ISchematicRepository<TState, TInput>
        , IMachineRepository<TState, TInput>
    {
        public Repository(REstateDbContextFactory dbContextFactory)
        {
            DbContextFactory = dbContextFactory;
        }

        public ISchematicRepository<TState, TInput> Schematics => this;
        public IMachineRepository<TState, TInput> Machines => this;

        private REstateDbContextFactory DbContextFactory { get; }

        /// <inheritdoc />
        public async Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName,
            CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            Schematic result;

            using(var context = DbContextFactory.CreateContext())
            try
            {
                result = await context.Schematics.SingleAsync(
                    schematicRecord => schematicRecord.SchematicName == schematicName,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw new SchematicDoesNotExistException(schematicName);
            }
            

            var schematic = result.SchematicBytes.ToSchematic<TState, TInput>();

            return schematic;
        }

        /// <inheritdoc />
        public async Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));
            if (schematic.SchematicName == null)
                throw new ArgumentException("Schematic must have a name to be stored.", nameof(schematic));


            var schematicBytes = schematic.ToSchematicRepresentation();

            var record = new Schematic
            {
                SchematicName = schematic.SchematicName,
                SchematicBytes = schematicBytes
            };

            using (var context = DbContextFactory.CreateContext())
            {

                context.Schematics.Add(record);

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return schematic;
        }

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> CreateMachineAsync(string schematicName, string machineId,
            IDictionary<string, string> metadata,
            CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            var schematic = await RetrieveSchematicAsync(schematicName, cancellationToken).ConfigureAwait(false);

            return await CreateMachineAsync(schematic, machineId, metadata, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            string machineId,
            IDictionary<string, string> metadata,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));

            var id = machineId ?? Guid.NewGuid().ToString();

            var schematicBytes = schematic.ToSchematicRepresentation();

            var stateJson = schematic.InitialState.ToStateRepresentation();

            const long commitNumber = 0L;
            var updatedTime = DateTimeOffset.UtcNow;

            var record = new Machine
            {
                MachineId = id,
                SchematicName = schematic.SchematicName,
                SchematicBytes = schematicBytes,
                StateJson = stateJson,
                CommitNumber = commitNumber,
                UpdatedTime = updatedTime
            };

            if (metadata != null)
            {
                record.MetadataEntries = metadata.Select(kvp => new MetadataEntry
                {
                    Key = kvp.Key,
                    Value = kvp.Value
                }).ToList();
            }

            using (var context = DbContextFactory.CreateContext())
            {
                context.Machines.Add(record);

                try
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException dbEx)
                    when (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    throw new MachineAlreadyExistException(record.MachineId);
                }
            }

            return new MachineStatus<TState, TInput>
            {
                MachineId = id,
                Schematic = schematic.Clone(),
                State = schematic.InitialState,
                Metadata = metadata,
                CommitNumber = commitNumber,
                UpdatedTime = updatedTime,
                StateBag = new Dictionary<string, string>(0)
            };
        }

        /// <inheritdoc />
        public async Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));

            var schematicBytes = schematic.ToSchematicRepresentation();

            var stateJson = schematic.InitialState.ToStateRepresentation();

            const long commitNumber = 0L;
            var updatedTime = DateTimeOffset.UtcNow;

            var records = new List<Machine>();
            var machineStatuses = new List<MachineStatus<TState, TInput>>();

            foreach (var dictionary in metadata)
            {
                var machineId = Guid.NewGuid().ToString();

                List<MetadataEntry> metadataEntries = null;
                
                if (dictionary != null)
                {
                    metadataEntries = dictionary.Select(kvp => new MetadataEntry
                    {
                        Key = kvp.Key,
                        Value = kvp.Value
                    }).ToList();
                }

                records.Add(new Machine
                {
                    MachineId = machineId,
                    SchematicBytes = schematicBytes,
                    StateJson = stateJson,
                    CommitNumber = commitNumber,
                    UpdatedTime = updatedTime,
                    MetadataEntries = metadataEntries
                });

                machineStatuses.Add(new MachineStatus<TState, TInput>
                {
                    MachineId = machineId,
                    Schematic = schematic.Clone(),
                    State = schematic.InitialState,
                    Metadata = dictionary,
                    CommitNumber = commitNumber,
                    UpdatedTime = updatedTime,
                    StateBag = new Dictionary<string, string>(0)
                });
            }

            using (var context = DbContextFactory.CreateContext())
            {
                await context.AddRangeAsync(records, cancellationToken);

                await context.SaveChangesAsync(cancellationToken);
            }

            return machineStatuses;
        }

        public async Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            var schematic = await RetrieveSchematicAsync(schematicName, cancellationToken).ConfigureAwait(false);

            return await BulkCreateMachinesAsync(schematic, metadata, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            using (var context = DbContextFactory.CreateContext())
            {
                var machineRecord = new Machine
                {
                    MachineId = machineId
                };

                context.Machines.Attach(machineRecord);
                
                context.Machines.Remove(machineRecord);

                try
                {
                    await context.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // record didn't exist
                }
                
            }
        }

        #region IDisposable Support

        // To detect redundant calls
        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}