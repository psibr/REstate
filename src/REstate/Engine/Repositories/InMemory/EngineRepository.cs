using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Repositories.InMemory
{
    using Metadata = IDictionary<string, string>;

    public class EngineRepository<TState, TInput>
        : ISchematicRepository<TState, TInput>
        , IMachineRepository<TState, TInput>
    {
        private IDictionary<string, Schematic<TState, TInput>> Schematics { get; } =
            new Dictionary<string, Schematic<TState, TInput>>();
        private IDictionary<string, (MachineStatus<TState, TInput> MachineStatus, Metadata Metadata)> Machines { get; } =
            new Dictionary<string, (MachineStatus<TState, TInput>, Metadata)>();

        /// <summary>
        /// Retrieves a previously stored Schematic by name.
        /// </summary>
        /// <param name="schematicName">The name of the Schematic</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schematicName"/> is null.</exception>
        /// <exception cref="SchematicDoesNotExistException">Thrown when no matching Schematic was found for the given name.</exception>
        public Task<Schematic<TState, TInput>> RetrieveSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            if (!Schematics.TryGetValue(schematicName, out var schematic))
                throw new SchematicDoesNotExistException(schematicName);

            return Task.FromResult(schematic);
        }

        /// <summary>
        /// Stores a Schematic, using its <c>SchematicName</c> as the key.
        /// </summary>
        /// <param name="schematic">The Schematic to store</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schematic"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="schematic"/> has a null <c>SchematicName</c> property.</exception>
        public Task<Schematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));
            if (schematic.SchematicName == null) throw new ArgumentException("Schematic must have a name to be stored.", nameof(schematic));

            Schematics.Add(schematic.SchematicName, schematic);

            var storedSchematic = Schematics[schematic.SchematicName];

            return Task.FromResult(storedSchematic);
        }

        /// <summary>
        /// Creates a new Machine from a provided Schematic.
        /// </summary>
        /// <param name="schematicName">The name of the stored Schematic</param>
        /// <param name="machineId">The Id of Machine to create; if null, an Id will be generated.</param>
        /// <param name="metadata">Related metadata for the Machine</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schematicName"/> is null.</exception>
        public Task<MachineStatus<TState, TInput>> CreateMachineAsync(
            string schematicName,
            string machineId,
            Metadata metadata,
            CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            if (!Schematics.TryGetValue(schematicName, out var schematic))
                throw new SchematicDoesNotExistException(schematicName);

            return CreateMachineAsync(schematic, machineId, metadata, cancellationToken);
        }

        /// <summary>
        /// Creates a new Machine from a provided Schematic.
        /// </summary>
        /// <param name="schematic">The Schematic of the Machine</param>
        /// <param name="machineId">The Id of Machine to create; if null, an Id will be generated.</param>
        /// <param name="metadata">Related metadata for the Machine</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schematic"/> is null.</exception>
        /// <exception cref="SchematicDoesNotExistException">Thrown when no matching Schematic was found for the given name.</exception>
        public Task<MachineStatus<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            string machineId,
            Metadata metadata,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));

            var id = machineId ?? Guid.NewGuid().ToString();

            var record = new MachineStatus<TState, TInput>
            {
                MachineId = id,
                Schematic = schematic,
                State = schematic.InitialState,
                CommitNumber = 0L,
                UpdatedTime = DateTime.UtcNow,
                Metadata = metadata
            };

            Machines.Add(id, (record, metadata));

            return Task.FromResult(record);
        }

        public Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<Metadata> metadata,
            CancellationToken cancellationToken = new CancellationToken())
        {
            List<(MachineStatus<TState, TInput> MachineStatus, IDictionary<string, string> Metadata)> machineRecords =
                metadata.Select(meta =>
                    (new MachineStatus<TState, TInput>
                    {
                        MachineId = Guid.NewGuid().ToString(),
                        Schematic = schematic,
                        State = schematic.InitialState,
                        CommitNumber = 0L,
                        UpdatedTime = DateTime.UtcNow,
                        Metadata = meta
                    },
                    meta)).ToList();

            foreach (var machineRecord in machineRecords)
            {
                Machines.Add(machineRecord.MachineStatus.MachineId, machineRecord);
            }

            return Task.FromResult<ICollection<MachineStatus<TState, TInput>>>(
                machineRecords.Select(r => r.MachineStatus).ToList());
        }

        public Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<Metadata> metadata,
            CancellationToken cancellationToken = default)
        {
            var schematic = Schematics[schematicName];

            return BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
        }

        /// <summary>
        /// Deletes a Machine.
        /// </summary>
        /// <remarks>
        /// Does not throw an exception if a matching Machine was not found.
        /// </remarks>
        /// <param name="machineId">The Id of the Machine to delete</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="machineId"/> is null.</exception>
        public Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            Machines.Remove(machineId);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        /// <summary>
        /// Retrieves the record for a Machine Status.
        /// </summary>
        /// <param name="machineId">The Id of the Machine</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="machineId"/> is null.</exception>
        /// <exception cref="MachineDoesNotExistException">Thrown when no matching MachineId was found.</exception>
        public Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            if (!Machines.TryGetValue(machineId, out var record))
                throw new MachineDoesNotExistException(machineId);

            return Task.FromResult(record.MachineStatus);
        }

        /// <summary>
        /// Updates the Status record of a Machine
        /// </summary>
        /// <param name="machineId">The Id of the Machine</param>
        /// <param name="state">The state to which the Status is set.</param>
        /// <param name="lastCommitNumber">
        /// If provided, will guarentee the update will occur only 
        /// if the value matches the current Status's CommitNumber.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="machineId"/> is null.</exception>
        /// <exception cref="MachineDoesNotExistException">Thrown when no matching MachineId was found.</exception>
        /// <exception cref="StateConflictException">Thrown when a conflict occured on CommitNumber; no update was performed.</exception>
        public Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            string machineId,
            TState state,
            long? lastCommitNumber,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            if (!Machines.TryGetValue(machineId, out var record))
                throw new MachineDoesNotExistException(machineId);

            lock (record.MachineStatus)
            {
                if (lastCommitNumber == null || record.MachineStatus.CommitNumber == lastCommitNumber)
                {
                    record.MachineStatus.State = state;
                    record.MachineStatus.CommitNumber++;
                    record.MachineStatus.UpdatedTime = DateTimeOffset.UtcNow;
                }
                else
                {
                    throw new StateConflictException();
                }
            }

            return Task.FromResult(record.MachineStatus);
        }
    }
}
