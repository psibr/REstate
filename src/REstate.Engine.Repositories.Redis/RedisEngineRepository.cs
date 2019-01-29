using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Schematics;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    using Metadata = IDictionary<string, string>;

    internal class RedisEngineRepository<TState, TInput>
        : ISchematicRepository<TState, TInput>
        , IMachineRepository<TState, TInput>
    {
        internal const string SchematicKeyPrefix = "REstate/Schematics";
        internal const string MachinesKeyPrefix = "REstate/Machines";

        internal const string MachineSchematicsKeyPrefix = "REstate/MachineSchematics";

        private readonly IDatabase _restateDatabase;

        public RedisEngineRepository(IDatabase restateDatabase)
        {
            _restateDatabase = restateDatabase;
        }

        /// <inheritdoc />
        public async Task<Schematic<TState, TInput>> RetrieveSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default)
        {
            if (schematicName == null) throw new ArgumentNullException(nameof(schematicName));

            var value = await _restateDatabase.StringGetAsync($"{SchematicKeyPrefix}/{schematicName}").ConfigureAwait(false);

            var schematic = LZ4MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(value,
                ContractlessStandardResolver.Instance);

            return schematic;
        }

        /// <inheritdoc />
        public async Task<Schematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));
            if (schematic.SchematicName == null) throw new ArgumentException("Schematic must have a name to be stored.", nameof(schematic));

            var schematicBytes = LZ4MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);

            await _restateDatabase.StringSetAsync($"{SchematicKeyPrefix}/{schematic.SchematicName}", schematicBytes);

            return schematic;
        }

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> CreateMachineAsync(
            string schematicName,
            string machineId,
            Metadata metadata,
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
            Metadata metadata,
            CancellationToken cancellationToken = default)
        {
            if (schematic == null) throw new ArgumentNullException(nameof(schematic));

            var id = machineId ?? Guid.NewGuid().ToString();

            var schematicBytes = LZ4MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);

            var hash = Convert.ToBase64String(Murmur3.ComputeHashBytes(schematicBytes));

            var schematicResult = await _restateDatabase.StringGetAsync($"{MachineSchematicsKeyPrefix}/{hash}");

            if (!schematicResult.HasValue)
            {
                await _restateDatabase.StringSetAsync($"{MachineSchematicsKeyPrefix}/{hash}", schematicBytes);
            }

            const int commitNumber = 0;
            var updatedTime = DateTimeOffset.UtcNow;

            var record = new RedisMachineStatus<TState, TInput>
            {
                MachineId = id,
                SchematicHash = hash,
                State = schematic.InitialState,
                CommitNumber = commitNumber,
                UpdatedTime = updatedTime,
                Metadata = metadata,
                StateBag = new Dictionary<string, string>()
            };

            var recordBytes = MessagePackSerializer.Serialize(record);

            await _restateDatabase.StringSetAsync($"{MachinesKeyPrefix}/{machineId}", recordBytes, null, When.NotExists).ConfigureAwait(false);

            return new MachineStatus<TState, TInput>
            {
                MachineId = machineId,
                Schematic = schematic,
                State = schematic.InitialState,
                CommitNumber = commitNumber,
                UpdatedTime = updatedTime,
                Metadata = metadata,
                StateBag = record.StateBag
            };
        }

        /// <inheritdoc />
        public async Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<Metadata> metadata,
            CancellationToken cancellationToken = default)
        {
            var schematicBytes = LZ4MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);

            var hash = Convert.ToBase64String(Murmur3.ComputeHashBytes(schematicBytes));

            const int commitNumber = 0;

            var machineRecords =
                metadata.Select(meta =>
                    new MachineStatus<TState, TInput>
                    {
                        MachineId = Guid.NewGuid().ToString(),
                        Schematic = schematic,
                        State = schematic.InitialState,
                        CommitNumber = commitNumber,
                        UpdatedTime = DateTime.UtcNow,
                        Metadata = meta,
                        StateBag = new Dictionary<string, string>()
                    }).ToList();

            await _restateDatabase.StringSetAsync($"{MachineSchematicsKeyPrefix}/{hash}", schematicBytes, null, When.NotExists);

            var batchOps = machineRecords.Select(s =>
                _restateDatabase.StringSetAsync($"{MachinesKeyPrefix}/{s.MachineId}", MessagePackSerializer.Serialize(
                    new RedisMachineStatus<TState, TInput>
                    {
                        MachineId = s.MachineId,
                        SchematicHash = hash,
                        State = s.State,
                        CommitNumber = s.CommitNumber,
                        UpdatedTime = s.UpdatedTime,
                        Metadata = s.Metadata,
                        StateBag = s.StateBag
                    }))
            );

            await Task.WhenAll(batchOps).ConfigureAwait(false);

            return machineRecords;
        }

        /// <inheritdoc />
        public async Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<Metadata> metadata,
            CancellationToken cancellationToken = default)
        {
            var schematic = await RetrieveSchematicAsync(schematicName, cancellationToken).ConfigureAwait(false);

            
            return await BulkCreateMachinesAsync(schematic, metadata, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            if (machineId == null) throw new ArgumentNullException(nameof(machineId));

            await _restateDatabase.KeyDeleteAsync($"{MachinesKeyPrefix}/{machineId}").ConfigureAwait(false);
        }
    }
}
