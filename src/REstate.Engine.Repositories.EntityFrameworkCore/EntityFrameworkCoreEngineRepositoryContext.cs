using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.EntityFrameworkCore;
using REstate.Schematics;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public class EntityFrameworkCoreEngineRepositoryContext<TState, TInput>
        : IEngineRepositoryContext<TState, TInput>
        , ISchematicRepository<TState, TInput>
        , IMachineRepository<TState, TInput>
    {
        public EntityFrameworkCoreEngineRepositoryContext(REstateDbContext dbContext)
        {
            DbContext = dbContext;
        }


        public ISchematicRepository<TState, TInput> Schematics => this;
        public IMachineRepository<TState, TInput> Machines => this;

        protected REstateDbContext DbContext { get; }

        /// <summary>
        /// Retrieves a previously stored Schematic by name.
        /// </summary>
        /// <param name="schematicName">The name of the Schematic</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="schematicName"/> is null.</exception>
        /// <exception cref="SchematicDoesNotExistException">Thrown when no matching Schematic was found for the given name.</exception>
        public async Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            EntityFrameworkCoreSchematic result;
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

        /// <summary>
        /// Stores a Schematic, using its <c>SchematicName</c> as the key.
        /// </summary>
        /// <param name="schematic">The Schematic to store</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="schematic"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="schematic"/> has a null <c>SchematicName</c> property.</exception>
        public async Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));
            if (schematic.SchematicName == null) throw new ArgumentException("Schematic must have a name to be stored.", nameof(schematic));


            var schematicJson = MessagePackSerializer.ToJson(
                obj: schematic, 
                resolver: ContractlessStandardResolver.Instance);

            var record = new EntityFrameworkCoreSchematic
            {
                SchematicName = schematic.SchematicName,
                SchematicJson = schematicJson
            };

            DbContext.Schematics.Add(record);

            await DbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return schematic;
        }

        /// <summary>
        /// Creates a new Machine from a provided Schematic.
        /// </summary>
        /// <param name="schematicName">The name of the stored Schematic</param>
        /// <param name="machineId">The Id of Machine to create; if null, an Id will be generated.</param>
        /// <param name="metadata">Related metadata for the Machine</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="schematicName"/> is null.</exception>
        public async Task<MachineStatus<TState, TInput>> CreateMachineAsync(string schematicName, string machineId, IDictionary<string, string> metadata,
            CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            var schematic = await RetrieveSchematicAsync(schematicName, cancellationToken).ConfigureAwait(false);

            return await CreateMachineAsync(schematic, machineId, metadata, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new Machine from a provided Schematic.
        /// </summary>
        /// <param name="schematic">The Schematic of the Machine</param>
        /// <param name="machineId">The Id of Machine to create; if null, an Id will be generated.</param>
        /// <param name="metadata">Related metadata for the Machine</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="schematic"/> is null.</exception>
        /// <exception cref="SchematicDoesNotExistException">Thrown when no matching Schematic was found for the given name.</exception>
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

            string metadataJson = null;
            if (metadata != null)
                metadataJson = MessagePackSerializer.ToJson(
                    obj: metadata);

            var commitTag = Guid.NewGuid();
            var updatedTime = DateTimeOffset.UtcNow;

            var record = new EntityFrameworkCoreMachineStatus
            {
                MachineId = id,
                SchematicJson = schematicJson,
                StateJson = stateJson,
                CommitTag = commitTag,
                UpdatedTime = updatedTime,
                MetadataJson = metadataJson
            };

            DbContext.Machines.Add(record);

            await DbContext.SaveChangesAsync(cancellationToken);

            return new MachineStatus<TState, TInput>
            {
                MachineId = id,
                Schematic = schematic.Clone(),
                State = schematic.InitialState,
                Metadata = metadata,
                CommitTag = commitTag,
                UpdatedTime = updatedTime
            };
        }

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

            var commitTag = Guid.NewGuid();
            var updatedTime = DateTimeOffset.UtcNow;

            var records = new List<EntityFrameworkCoreMachineStatus>();
            var machineStatuses = new List<MachineStatus<TState, TInput>>();

            foreach (var dictionary in metadata)
            {
                var machineId = Guid.NewGuid().ToString();

                string metadataJson = null;
                if (dictionary != null)
                    metadataJson = MessagePackSerializer.ToJson(
                        obj: dictionary);

                records.Add(new EntityFrameworkCoreMachineStatus
                {
                    MachineId = machineId,
                    SchematicJson = schematicJson,
                    StateJson = stateJson,
                    CommitTag = commitTag,
                    UpdatedTime = updatedTime,
                    MetadataJson = metadataJson
                });

                machineStatuses.Add(new MachineStatus<TState, TInput>
                {
                    MachineId = machineId,
                    Schematic = schematic.Clone(),
                    State = schematic.InitialState,
                    Metadata = dictionary,
                    CommitTag = commitTag,
                    UpdatedTime = updatedTime
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

        /// <summary>
        /// Deletes a Machine.
        /// </summary>
        /// <remarks>
        /// Does not throw an exception if a matching Machine was not found.
        /// </remarks>
        /// <param name="machineId">The Id of the Machine to delete</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="machineId"/> is null.</exception>
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

        /// <summary>
        /// Retrieves the record for a Machine Status.
        /// </summary>
        /// <param name="machineId">The Id of the Machine</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="machineId"/> is null.</exception>
        /// <exception cref="MachineDoesNotExistException">Thrown when no matching MachineId was found.</exception>
        public async Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(
            string machineId, 
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            var machineRecord = await DbContext.Machines
                .SingleOrDefaultAsync(
                    status => status.MachineId == machineId,
                    cancellationToken).ConfigureAwait(false);

            if(machineRecord == null) throw new MachineDoesNotExistException(machineId);

            var schematic = MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                bytes: MessagePackSerializer.FromJson(machineRecord.SchematicJson), 
                resolver: ContractlessStandardResolver.Instance);

            var state = MessagePackSerializer.Deserialize<TState>(
                bytes: MessagePackSerializer.FromJson(machineRecord.StateJson),
                resolver: ContractlessStandardResolver.Instance);

            IDictionary<string, string> metadata = null;
            if(machineRecord.MetadataJson != null)
                metadata = MessagePackSerializer.Deserialize<IDictionary<string, string>>(
                    bytes: MessagePackSerializer.FromJson(machineRecord.MetadataJson));

            return new MachineStatus<TState, TInput>
            {
                MachineId = machineId,
                Schematic = schematic,
                State = state,
                Metadata = metadata,
                CommitTag = machineRecord.CommitTag,
                UpdatedTime = machineRecord.UpdatedTime
            };
        }

        /// <summary>
        /// Updates the Status record of a Machine
        /// </summary>
        /// <param name="machineId">The Id of the Machine</param>
        /// <param name="state">The state to which the Status is set.</param>
        /// <param name="lastCommitTag">
        /// If provided, will guarentee the update will occur only 
        /// if the value matches the current Status's CommitTag.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="machineId"/> is null.</exception>
        /// <exception cref="MachineDoesNotExistException">Thrown when no matching MachineId was found.</exception>
        /// <exception cref="StateConflictException">Thrown when a conflict occured on CommitTag; no update was performed.</exception>
        public async Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            string machineId,
            TState state,
            Guid? lastCommitTag,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            var machineRecord = await DbContext.Machines
                .SingleOrDefaultAsync(
                    status => status.MachineId == machineId,
                    cancellationToken).ConfigureAwait(false);

            if(machineRecord == null) throw new MachineDoesNotExistException(machineId);

            if (lastCommitTag == null || machineRecord.CommitTag == lastCommitTag)
            {
                var stateJson = MessagePackSerializer.ToJson(
                    obj: state, 
                    resolver: ContractlessStandardResolver.Instance);

                machineRecord.StateJson = stateJson;
                machineRecord.CommitTag = Guid.NewGuid();
                machineRecord.UpdatedTime = DateTimeOffset.UtcNow;
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

            IDictionary<string, string> metadata = null;
            if(machineRecord.MetadataJson != null)
                metadata = MessagePackSerializer.Deserialize<IDictionary<string, string>>(
                    bytes: MessagePackSerializer.FromJson(machineRecord.MetadataJson));

            return new MachineStatus<TState, TInput>
            {
                MachineId = machineId,
                Schematic = schematic,
                State = state,
                Metadata = metadata,
                CommitTag = machineRecord.CommitTag,
                UpdatedTime = machineRecord.UpdatedTime
            };
        }

        #region IDisposable Support
        // To detect redundant calls
        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
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
