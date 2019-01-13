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
        , IOptimisticallyConcurrentStateRepository<TState, TInput>
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

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            Machine machineRecord;

            using (var context = DbContextFactory.CreateContext())
            { 
                 machineRecord = await context.Machines
                    .Include(machine => machine.MetadataEntries)
                    .Include(machine => machine.StateBagEntries)
                    .SingleOrDefaultAsync(
                        status => status.MachineId == machineId,
                        cancellationToken).ConfigureAwait(false);
            }

            if (machineRecord == null) throw new MachineDoesNotExistException(machineId);

            var schematic = machineRecord.SchematicBytes.ToSchematic<TState, TInput>();

            var state = machineRecord.StateJson.ToState<TState>();

            var metadata = machineRecord.MetadataEntries
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            var stateBag = machineRecord.StateBagEntries
                .ToDictionary(entry => entry.Key, entry => entry.Value);            
            
            return new MachineStatus<TState, TInput>
            {
                MachineId = machineId,
                Schematic = schematic,
                State = state,
                Metadata = metadata,
                CommitNumber = machineRecord.CommitNumber,
                UpdatedTime = machineRecord.UpdatedTime,
                StateBag = stateBag
            };
        }

        async Task<MachineStatus<TState, TInput>> IOptimisticallyConcurrentStateRepository<TState, TInput>.SetMachineStateAsync(
                MachineStatus<TState, TInput> machineStatus,
                TState state,
                long? lastCommitNumber,
                IDictionary<string, string> stateBag,
                CancellationToken cancellationToken)
        {
            if(machineStatus == null) throw  new ArgumentNullException(nameof(machineStatus));
            if (machineStatus.MachineId == null) throw new ArgumentException("MachineId must be provided", nameof(machineStatus));

            var machineRecord = new Machine
            {
                MachineId = machineStatus.MachineId,
                CommitNumber = machineStatus.CommitNumber
            };

            var newStatus = new MachineStatus<TState, TInput>
            {
                MachineId = machineStatus.MachineId,
                Schematic = machineStatus.Schematic,
                State = state,
                Metadata = machineStatus.Metadata
            };

            using (var context = DbContextFactory.CreateContext())
            {

                if (lastCommitNumber == null || machineStatus.CommitNumber == lastCommitNumber)
                {
                    context.Machines.Attach(machineRecord);

                    var stateJson = state.ToStateRepresentation();

                    machineRecord.StateJson = stateJson;
                    machineRecord.CommitNumber++;
                    machineRecord.UpdatedTime = DateTimeOffset.UtcNow;

                    if (stateBag != null && lastCommitNumber != null)
                    {
                        var inserts = stateBag.Keys.Except(machineStatus.StateBag.Keys).Select(k => new StateBagEntry { Key = k, MachineId = machineStatus.MachineId, Value = stateBag[k] });
                        var updates = machineStatus.StateBag.Keys.Intersect(stateBag.Keys).Select(k => new StateBagEntry { Key = k, MachineId = machineStatus.MachineId}).ToList();
                        var deletes = machineStatus.StateBag.Keys.Except(stateBag.Keys).Select(k => new StateBagEntry { Key = k, MachineId = machineStatus.MachineId }).ToList();

                        context.StateBagEntries.AddRange(inserts);

                        context.AttachRange(updates);

                        foreach (var stateBagEntry in updates)
                        {
                            stateBagEntry.Value = stateBag[stateBagEntry.Key];
                        }

                        context.StateBagEntries.AttachRange(deletes);
                        context.StateBagEntries.RemoveRange(deletes);

                        newStatus.StateBag = stateBag;
                    }
                    else
                    {
                        newStatus.StateBag = machineStatus.StateBag;
                    }
                }
                else
                {
                    throw new StateConflictException();
                }

                try
                {
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new StateConflictException(ex);
                }
            }

            newStatus.CommitNumber = machineRecord.CommitNumber;
            newStatus.UpdatedTime = machineRecord.UpdatedTime;

            return newStatus;
        }

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            string machineId,
            TState state,
            long? lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            Machine machineRecord;
            using (var context = DbContextFactory.CreateContext())
            {
                machineRecord = await context.Machines
                    .Include(machine => machine.MetadataEntries)
                    .Include(machine => machine.StateBagEntries)
                    .SingleOrDefaultAsync(
                        status => status.MachineId == machineId,
                        cancellationToken).ConfigureAwait(false);
            }

            if (machineRecord == null) throw new MachineDoesNotExistException(machineId);

            var schematic = machineRecord.SchematicBytes.ToSchematic<TState, TInput>();

            var metadata = machineRecord.MetadataEntries?.ToDictionary(entry => entry.Key, entry => entry.Value);

            var currentStateBag = machineRecord.StateBagEntries?.ToDictionary(entry => entry.Key, entry => entry.Value);

            var machineStatus = new MachineStatus<TState, TInput>
            {
                MachineId = machineId,
                Schematic = schematic,
                State = state,
                Metadata = metadata,
                CommitNumber = machineRecord.CommitNumber,
                UpdatedTime = machineRecord.UpdatedTime,
                StateBag = currentStateBag
            };

            return await (this as IOptimisticallyConcurrentStateRepository<TState, TInput>)
                .SetMachineStateAsync(machineStatus, state, lastCommitNumber, stateBag, cancellationToken)
                .ConfigureAwait(false);
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