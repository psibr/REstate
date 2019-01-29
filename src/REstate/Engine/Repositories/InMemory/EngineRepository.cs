using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<string, Schematic<TState, TInput>> Schematics { get; } = new ConcurrentDictionary<string, Schematic<TState, TInput>>();
        private ConcurrentDictionary<string, MachineStatus<TState, TInput>> Machines { get; } =
            new ConcurrentDictionary<string, MachineStatus<TState, TInput>>();

        /// <inheritdoc />
        public Task<Schematic<TState, TInput>> RetrieveSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            if (!Schematics.TryGetValue(schematicName, out var schematic))
                throw new SchematicDoesNotExistException(schematicName);

            return Task.FromResult(schematic);
        }

        /// <inheritdoc />
        public Task<Schematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));
            if (schematic.SchematicName == null) throw new ArgumentException("Schematic must have a name to be stored.", nameof(schematic));

            Schematics.AddOrUpdate(schematic.SchematicName, (key) => schematic, (key, old) => schematic);

            var storedSchematic = Schematics[schematic.SchematicName];

            return Task.FromResult(storedSchematic);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                Metadata = metadata,
                StateBag = new Dictionary<string, string>()
            };

            if(Machines.TryAdd(id, record))
                return Task.FromResult(record);

            throw new MachineAlreadyExistException(id);
        }

        /// <inheritdoc />
        public Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<Metadata> metadata,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var exceptions = new Queue<Exception>();

            var aggregateException = new AggregateException(exceptions);

            List<MachineStatus<TState, TInput>> machineRecords =
                metadata.Select(meta =>
                    new MachineStatus<TState, TInput>
                    {
                        MachineId = Guid.NewGuid().ToString(),
                        Schematic = schematic,
                        State = schematic.InitialState,
                        CommitNumber = 0L,
                        UpdatedTime = DateTime.UtcNow,
                        Metadata = meta,
                        StateBag = new Dictionary<string, string>()
                    }).ToList();

            foreach (var machineRecord in machineRecords)
            {
                if (!Machines.TryAdd(machineRecord.MachineId, machineRecord))
                    exceptions.Enqueue(new MachineAlreadyExistException(machineRecord.MachineId));
            }

            if(exceptions.Count > 0)
                throw aggregateException;

            return Task.FromResult<ICollection<MachineStatus<TState, TInput>>>(
                machineRecords.ToList());
        }

        /// <inheritdoc />
        public Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<Metadata> metadata,
            CancellationToken cancellationToken = default)
        {
            var schematic = Schematics[schematicName];

            return BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            Machines.TryRemove(machineId, out var _);

#if NET45
            return Task.FromResult(0);
#else
            return Task.CompletedTask;
#endif
        }

        public Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            if (!Machines.TryGetValue(machineId, out var record))
                throw new MachineDoesNotExistException(machineId);

            return Task.FromResult(record);
        }
    }
}
