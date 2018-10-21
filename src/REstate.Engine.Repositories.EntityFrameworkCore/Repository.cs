using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using REstate.Schematics;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public class Repository<TState, TInput>
        : IEngineRepositoryContext<TState, TInput>
        , ISchematicRepository<TState, TInput>
        , IMachineRepository<TState, TInput>
    {
        public Repository(REstateDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public ISchematicRepository<TState, TInput> Schematics => this;
        public IMachineRepository<TState, TInput> Machines => this;

        private REstateDbContext DbContext { get; }

        /// <inheritdoc />
        public async Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName,
            CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            Schematic result;
            try
            {
                result = await DbContext.Schematics.SingleAsync(
                    schematicRecord => schematicRecord.SchematicName == schematicName,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw new SchematicDoesNotExistException(schematicName);
            }

            var schematic = MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                bytes: MessagePackSerializer.FromJson(result.SchematicJson),
                resolver: ContractlessStandardResolver.Instance);

            return schematic;
        }

        /// <inheritdoc />
        public async Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));
            if (schematic.SchematicName == null)
                throw new ArgumentException("Schematic must have a name to be stored.", nameof(schematic));


            var schematicJson = MessagePackSerializer.ToJson(
                obj: schematic,
                resolver: ContractlessStandardResolver.Instance);

            var record = new Schematic
            {
                SchematicName = schematic.SchematicName,
                SchematicJson = schematicJson
            };

            DbContext.Schematics.Add(record);

            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

            var schematicJson = MessagePackSerializer.ToJson(
                obj: schematic,
                resolver: ContractlessStandardResolver.Instance);

            var stateJson = MessagePackSerializer.ToJson(
                obj: schematic.InitialState,
                resolver: ContractlessStandardResolver.Instance);

            var commitNumber = 0L;
            var updatedTime = DateTimeOffset.UtcNow;

            var record = new Machine
            {
                MachineId = id,
                SchematicJson = schematicJson,
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

            DbContext.Machines.Add(record);

            try
            {
                await DbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException dbEx)
                when (dbEx.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
            {
                throw new MachineAlreadyExistException(record.MachineId);
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

            var schematicJson = MessagePackSerializer.ToJson(
                obj: schematic,
                resolver: ContractlessStandardResolver.Instance);

            var stateJson = MessagePackSerializer.ToJson(
                obj: schematic.InitialState,
                resolver: ContractlessStandardResolver.Instance);

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
                    SchematicJson = schematicJson,
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

            await DbContext.AddRangeAsync(records, cancellationToken);

            await DbContext.SaveChangesAsync(cancellationToken);

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

            var machineRecord = await DbContext.Machines
                .SingleOrDefaultAsync(
                    status => status.MachineId == machineId,
                    cancellationToken).ConfigureAwait(false);

            if (machineRecord != null)
            {
                DbContext.Machines.Remove(machineRecord);

                await DbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            var machineRecord = await DbContext.Machines
                .Include(machine => machine.MetadataEntries)
                .Include(machine => machine.StateBagEntries)
                .SingleOrDefaultAsync(
                    status => status.MachineId == machineId,
                    cancellationToken).ConfigureAwait(false);

            if (machineRecord == null) throw new MachineDoesNotExistException(machineId);

            var schematic = MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                bytes: MessagePackSerializer.FromJson(machineRecord.SchematicJson),
                resolver: ContractlessStandardResolver.Instance);

            var state = MessagePackSerializer.Deserialize<TState>(
                bytes: MessagePackSerializer.FromJson(machineRecord.StateJson),
                resolver: ContractlessStandardResolver.Instance);

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

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            string machineId,
            TState state,
            long? lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            var machineRecord = await DbContext.Machines
                .Include(machine => machine.MetadataEntries)
                .Include(machine => machine.StateBagEntries)
                .SingleOrDefaultAsync(
                    status => status.MachineId == machineId,
                    cancellationToken).ConfigureAwait(false);

            if (machineRecord == null) throw new MachineDoesNotExistException(machineId);

            if (lastCommitNumber == null || machineRecord.CommitNumber == lastCommitNumber)
            {
                var stateJson = MessagePackSerializer.ToJson(
                    obj: state,
                    resolver: ContractlessStandardResolver.Instance);

                machineRecord.StateJson = stateJson;
                machineRecord.CommitNumber++;
                machineRecord.UpdatedTime = DateTimeOffset.UtcNow;

                if (stateBag != null && lastCommitNumber != null)
                    machineRecord.StateBagEntries = stateBag.Select(kvp => new StateBagEntry
                    {
                        Key = kvp.Key,
                        Value = kvp.Value
                    }).ToList();
            }
            else
            {
                throw new StateConflictException();
            }

            try
            {
                await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new StateConflictException(ex);
            }

            var schematic = MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                bytes: MessagePackSerializer.FromJson(machineRecord.SchematicJson),
                resolver: ContractlessStandardResolver.Instance);

            var metadata = machineRecord.MetadataEntries?.ToDictionary(entry => entry.Key, entry => entry.Value);

            var currentStateBag = machineRecord.StateBagEntries?.ToDictionary(entry => entry.Key, entry => entry.Value);
            
            return new MachineStatus<TState, TInput>
            {
                MachineId = machineId,
                Schematic = schematic,
                State = state,
                Metadata = metadata,
                CommitNumber = machineRecord.CommitNumber,
                UpdatedTime = machineRecord.UpdatedTime,
                StateBag = currentStateBag
            };
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
                    DbContext.Dispose();
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