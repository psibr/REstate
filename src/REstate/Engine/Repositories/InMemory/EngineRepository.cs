using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepository<TState, TInput> 
        : ISchematicRepository<TState, TInput>
        , IMachineRepository<TState, TInput>
    {
        private static IDictionary<string, Schematic<TState, TInput>> Schematics { get; } = new Dictionary<string, Schematic<TState, TInput>>();
        private static IDictionary<string, ValueTuple<MachineStatus<TState, TInput>, IDictionary<string, string>>> Machines { get; } =
            new Dictionary<string, ValueTuple<MachineStatus<TState, TInput>, IDictionary<string, string>>>();

        public Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var schematic = Schematics[schematicName];

            return Task.FromResult(schematic);
        }

        public Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default(CancellationToken))
        {
            Schematics.Add(schematic.SchematicName, schematic);

            var storedSchematic = Schematics[schematic.SchematicName];

            return Task.FromResult(storedSchematic);
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

        public Task<MachineStatus<TState, TInput>> CreateMachineAsync(string schematicName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return CreateMachineAsync(schematicName, new Dictionary<string, string>(), cancellationToken);
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

            lock(machine)
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
