using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Schematics;
using StackExchange.Redis;

namespace REstate.Engine.Repositories.Redis
{
    internal class RedisMachineStatusStore<TState, TInput>
        : IMachineStatusStore<TState, TInput>
    {
        private readonly IDatabase _restateDatabase;
        private readonly string _machineId;

        public RedisMachineStatusStore(IDatabase database, string machineId)
        {
            _restateDatabase = database;
            _machineId = machineId ?? throw new ArgumentNullException(nameof(machineId));
        }

        public async Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(CancellationToken cancellationToken = default)
        {
            var machineBytes = await _restateDatabase.StringGetAsync($"{RedisEngineRepository<TState, TInput>.MachinesKeyPrefix}/{_machineId}").ConfigureAwait(false);

            if (!machineBytes.HasValue) throw new MachineDoesNotExistException(_machineId);

            var redisRecord = MessagePackSerializer.Deserialize<RedisMachineStatus<TState, TInput>>(
                machineBytes);

            var schematicBytes = await _restateDatabase
                .StringGetAsync($"{RedisEngineRepository<TState, TInput>.MachineSchematicsKeyPrefix}/{redisRecord.SchematicHash}").ConfigureAwait(false);

            var schematic = LZ4MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                schematicBytes,
                ContractlessStandardResolver.Instance);

            return new MachineStatus<TState, TInput>
            {
                MachineId = redisRecord.MachineId,
                Schematic = schematic,
                CommitNumber = redisRecord.CommitNumber,
                State = redisRecord.State,
                UpdatedTime = redisRecord.UpdatedTime,
                Metadata = redisRecord.Metadata,
                StateBag = redisRecord.StateBag
            };
        }

        public async Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            TState state,
            long? lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default)
        {
            var machineKey = $"{RedisEngineRepository<TState, TInput>.MachinesKeyPrefix}/{_machineId}";
            var machineBytes = await _restateDatabase.StringGetAsync(machineKey).ConfigureAwait(false);

            var status = MessagePackSerializer.Deserialize<RedisMachineStatus<TState, TInput>>(
                machineBytes);

            if (lastCommitNumber == null || status.CommitNumber == lastCommitNumber)
            {
                status.State = state;
                status.CommitNumber = status.CommitNumber++;
                status.UpdatedTime = DateTimeOffset.UtcNow;

                if (stateBag != null && lastCommitNumber != null)
                    status.StateBag = stateBag;
            }
            else
            {
                throw new StateConflictException();
            }

            var newMachineBytes = MessagePackSerializer.Serialize(status);

            var transaction = _restateDatabase.CreateTransaction();

            // Veify the record has not changed since read.
            transaction.AddCondition(Condition.StringEqual(machineKey, machineBytes));

            var setTask = transaction.StringSetAsync(machineKey, newMachineBytes);
            var schematicBytesTask = transaction.StringGetAsync($"{RedisEngineRepository<TState, TInput>.MachineSchematicsKeyPrefix}/{status.SchematicHash}");
            var committed = await transaction.ExecuteAsync().ConfigureAwait(false);

            await setTask.ConfigureAwait(false);

            // If the transaction failed, we had a conflict.
            if (!committed) throw new StateConflictException();

            var schematicBytes = await schematicBytesTask.ConfigureAwait(false);

            var schematic = LZ4MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(schematicBytes,
                ContractlessStandardResolver.Instance);

            return new MachineStatus<TState, TInput>
            {
                MachineId = status.MachineId,
                Schematic = schematic,
                State = status.State,
                CommitNumber = status.CommitNumber,
                UpdatedTime = status.UpdatedTime,
                Metadata = status.Metadata,
                StateBag = status.StateBag
            };
        }
    }
}
