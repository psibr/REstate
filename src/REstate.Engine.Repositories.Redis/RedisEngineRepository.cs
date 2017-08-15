using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public class RedisEngineRepository<TState, TInput>
        : ISchematicRepository<TState, TInput>
        , IMachineRepository<TState, TInput>
    {
        private const string SchematicKeyPrefix = "REstate/Schematics";
        private const string MachinesKeyPrefix = "REstate/Machines";

        private const string MachineSchematicsKeyPrefix = "REstate/MachineSchematics";

        private readonly IDatabase _restateDatabase;

        public RedisEngineRepository(IDatabase restateDatabase)
        {
            _restateDatabase = restateDatabase;
        }

        private static IDictionary<string, (MachineStatus<TState, TInput> MachineStatus, Metadata Metadata)> Machines { get; } =
            new Dictionary<string, (MachineStatus<TState, TInput>, Metadata)>();

        public async Task<Schematic<TState, TInput>> RetrieveSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var value = await _restateDatabase.StringGetAsync($"{SchematicKeyPrefix}/{schematicName}").ConfigureAwait(false);

            var schematic = LZ4MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(value,
                ContractlessStandardResolver.Instance);

            return schematic;
        }

        public async Task<Schematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var schematicBytes = LZ4MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);

            await _restateDatabase.StringSetAsync($"{SchematicKeyPrefix}/{schematic.SchematicName}", schematicBytes);

            return schematic;
        }

        public async Task<MachineStatus<TState, TInput>> CreateMachineAsync(
            string schematicName,
            Metadata metadata,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var schematic = await RetrieveSchematicAsync(schematicName, cancellationToken).ConfigureAwait(false);

            return await CreateMachineAsync(schematic, metadata, cancellationToken).ConfigureAwait(false);
        }

        public async Task<MachineStatus<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            Metadata metadata,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var machineId = Guid.NewGuid().ToString();

            var schematicBytes = LZ4MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);

            var hash = Convert.ToBase64String(Murmur3.ComputeHashBytes(schematicBytes));

            var schematicResult = await _restateDatabase.StringGetAsync($"{MachineSchematicsKeyPrefix}/{hash}");

            if (!schematicResult.HasValue)
            {
                await _restateDatabase.StringSetAsync($"{MachineSchematicsKeyPrefix}/{hash}", schematicBytes);
            }

            var commitTag = Guid.NewGuid();
            var updatedTime = DateTimeOffset.UtcNow;

            var record = new RedisMachineStatus<TState, TInput>
            {
                MachineId = machineId,
                SchematicHash = hash,
                State = schematic.InitialState,
                CommitTag = commitTag,
                UpdatedTime = updatedTime,
                Metadata = metadata
            };

            var recordBytes = MessagePackSerializer.Serialize(record);

            await _restateDatabase.StringSetAsync($"{MachinesKeyPrefix}/{machineId}", recordBytes).ConfigureAwait(false);

            return new MachineStatus<TState, TInput>
            {
                MachineId = machineId,
                Schematic = schematic,
                State = schematic.InitialState,
                CommitTag = commitTag,
                UpdatedTime = updatedTime,
                Metadata = metadata
            };
        }

        public async Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<Metadata> metadata,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var schematicBytes = LZ4MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);

            var hash = Convert.ToBase64String(Murmur3.ComputeHashBytes(schematicBytes));

            var machineRecords =
                metadata.Select(meta =>
                    new MachineStatus<TState, TInput>
                    {
                        MachineId = Guid.NewGuid().ToString(),
                        Schematic = schematic,
                        State = schematic.InitialState,
                        CommitTag = Guid.NewGuid(),
                        UpdatedTime = DateTime.UtcNow,
                        Metadata = meta
                    }).ToList();

            await _restateDatabase.StringSetAsync($"{MachineSchematicsKeyPrefix}/{hash}", schematicBytes, null, When.NotExists);

            var batchOps = machineRecords.Select(s =>
                _restateDatabase.StringSetAsync($"{MachinesKeyPrefix}/{s.MachineId}", MessagePackSerializer.Serialize(
                    new RedisMachineStatus<TState, TInput>
                    {
                        MachineId = s.MachineId,
                        SchematicHash = hash,
                        State = s.State,
                        CommitTag = s.CommitTag,
                        UpdatedTime = s.UpdatedTime,
                        Metadata = s.Metadata
                    }))
            );

            await Task.WhenAll(batchOps).ConfigureAwait(false);

            return machineRecords;
        }

        public async Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<Metadata> metadata,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var schematic = await RetrieveSchematicAsync(schematicName, cancellationToken).ConfigureAwait(false);

            
            return await BulkCreateMachinesAsync(schematic, metadata, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await _restateDatabase.KeyDeleteAsync($"{MachinesKeyPrefix}/{machineId}").ConfigureAwait(false);
        }

        public async Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(
            string machineId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var machineBytes = await _restateDatabase.StringGetAsync($"{MachinesKeyPrefix}/{machineId}").ConfigureAwait(false);

            var redisRecord = MessagePackSerializer.Deserialize<RedisMachineStatus<TState, TInput>>(
                machineBytes);

            var schematicBytes = await _restateDatabase
                .StringGetAsync($"{MachineSchematicsKeyPrefix}/{redisRecord.SchematicHash}").ConfigureAwait(false);

            var schematic = LZ4MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                schematicBytes,
                ContractlessStandardResolver.Instance);

            return new MachineStatus<TState, TInput>
            {
                MachineId = redisRecord.MachineId,
                Schematic = schematic,
                CommitTag = redisRecord.CommitTag,
                State = redisRecord.State,
                UpdatedTime = redisRecord.UpdatedTime,
                Metadata = redisRecord.Metadata
            };
        }

        public async Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            string machineId,
            TState state,
            Guid? lastCommitTag,
            CancellationToken cancellationToken = default(CancellationToken))
        {

            var machineKey = $"{MachinesKeyPrefix}/{machineId}";
            var machineBytes = await _restateDatabase.StringGetAsync(machineKey).ConfigureAwait(false);

            var status = MessagePackSerializer.Deserialize<RedisMachineStatus<TState, TInput>>(
                machineBytes);

            if (lastCommitTag == null || status.CommitTag == lastCommitTag)
            {
                status.State = state;
                status.CommitTag = Guid.NewGuid();
            }
            else
            {
                throw new StateConflictException("CommitTag did not match.");
            }

            var newMachineBytes = MessagePackSerializer.Serialize(status);
            
            var transaction = _restateDatabase.CreateTransaction();
            transaction.AddCondition(Condition.StringEqual(machineKey, machineBytes));
            var setTask = transaction.StringSetAsync(machineKey, newMachineBytes);
            var schematicBytesTask = transaction.StringGetAsync($"{MachineSchematicsKeyPrefix}/{status.SchematicHash}");
            var committed = await transaction.ExecuteAsync().ConfigureAwait(false);

            await setTask.ConfigureAwait(false);

            if(!committed)
                throw new StateConflictException("CommitTag did not match.");

            var schematicBytes = await schematicBytesTask.ConfigureAwait(false);

            var schematic = LZ4MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(schematicBytes,
                ContractlessStandardResolver.Instance);

            return new MachineStatus<TState, TInput>
            {
                MachineId = status.MachineId,
                Schematic = schematic,
                State = status.State,
                CommitTag = status.CommitTag,
                UpdatedTime = status.UpdatedTime,
                Metadata = status.Metadata
            };
        }
    }
}
