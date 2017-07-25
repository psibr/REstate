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
    public class RedisEngineRepository<TState, TInput>
        : ISchematicRepository<TState, TInput>
        , IMachineRepository<TState, TInput>
    {
        private readonly IDatabaseAsync _restateDatabase;

        public RedisEngineRepository(IDatabaseAsync restateDatabase)
        {
            _restateDatabase = restateDatabase;
        }

        private static IDictionary<string, Schematic<TState, TInput>> Schematics { get; } = new Dictionary<string, Schematic<TState, TInput>>();
        private static IDictionary<string, ValueTuple<MachineStatus<TState, TInput>, IDictionary<string, string>>> Machines { get; } =
            new Dictionary<string, ValueTuple<MachineStatus<TState, TInput>, IDictionary<string, string>>>();

        public async Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var value = await _restateDatabase.StringGetAsync(schematicName);

            var schematic = MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(value,
                ContractlessStandardResolver.Instance);

            return schematic;
        }

        public async Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _restateDatabase.StringSetAsync(schematic.SchematicName,
                MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance));

            return schematic;
        }

        public Task<MachineStatus<TState, TInput>> CreateMachineAsync(string schematicName, IDictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var schematic = Schematics[schematicName];

            return CreateMachineAsync(schematic, metadata, cancellationToken);
        }

        public Task<MachineStatus<TState, TInput>> CreateMachineAsync(Schematic<TState, TInput> schematic, IDictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var machineId = Guid.NewGuid().ToString();

            var record = new MachineStatus<TState, TInput>
            {
                MachineId = machineId,
                Schematic = schematic,
                State = schematic.InitialState,
                CommitTag = Guid.NewGuid(),
                StateChangedDateTime = DateTime.UtcNow
            };

            Machines.Add(machineId, (record, metadata));

            return Task.FromResult(record);
        }

        public Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(Schematic<TState, TInput> schematic, IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = new CancellationToken())
        {
            List<(MachineStatus<TState, TInput> MachineStatus, IDictionary<string, string> Metadata)> machineRecords = metadata.Select(m =>
                (new MachineStatus<TState, TInput>
                {
                    MachineId = Guid.NewGuid().ToString(),
                    Schematic = schematic,
                    State = schematic.InitialState,
                    CommitTag = Guid.NewGuid(),
                    StateChangedDateTime = DateTime.UtcNow
                },
                m)).ToList();

            foreach (var machineRecord in machineRecords)
            {
                Machines.Add(machineRecord.MachineStatus.MachineId, machineRecord);
            }

            return Task.FromResult<ICollection<MachineStatus<TState, TInput>>>(machineRecords.Select(r => r.MachineStatus).ToList());
        }

        public Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(string schematicName, IEnumerable<IDictionary<string, string>> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var schematic = Schematics[schematicName];

            return BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
        }

        public Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken = default(CancellationToken))
        {
            Machines.Remove(machineId);

            return Task.CompletedTask;
        }

        public Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(string machineId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var (machine, _) = Machines[machineId];

            return Task.FromResult(machine);
        }

        public Task<MachineStatus<TState, TInput>> SetMachineStateAsync(string machineId, TState state, TInput input, Guid? lastCommitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SetMachineStateAsync(machineId, state, input, null, lastCommitTag, cancellationToken);
        }

        public Task<MachineStatus<TState, TInput>> SetMachineStateAsync(string machineId, TState state, TInput input, string parameterData, Guid? lastCommitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            var (machine, _) = Machines[machineId];

            lock (machine)
            {
                if (lastCommitTag == null || machine.CommitTag == lastCommitTag)
                {
                    machine.State = state;
                    machine.CommitTag = Guid.NewGuid();
                }
                else
                {
                    throw new StateConflictException("CommitTag did not match.");
                }
            }

            return Task.FromResult(machine);
        }
    }
}
