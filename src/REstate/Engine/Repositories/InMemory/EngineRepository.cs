using REstate.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepository<TState, TInput> 
        : ISchematicRepository<TState, TInput>,
        IMachineRepository<TState, TInput>
    {
        private static IDictionary<string, Schematic<TState, TInput>> Schematics { get; } = new Dictionary<string, Schematic<TState, TInput>>();
        private static IDictionary<string, ValueTuple<MachineStatus<TState>, IDictionary<string, string>>> Machines { get; } = new Dictionary<string, ValueTuple<MachineStatus<TState>, IDictionary<string, string>>>();

        public Task<IEnumerable<Schematic<TState, TInput>>> ListSchematicsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Schematics.Values as IEnumerable<Schematic<TState, TInput>>);
        }

        public Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken)
        {
            var machineRecord = Schematics[schematicName];

            return Task.FromResult(machineRecord);
        }

        public Task<Schematic<TState, TInput>> RetrieveSchematicForMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            ValueTuple<MachineStatus<TState>, IDictionary<string, string>> instanceData;
            try
            {
                instanceData = Machines[machineId];
            }
            catch
            {
                throw new MachineDoesNotExistException();
            }

            try
            {
                var machineRecord = Schematics[instanceData.Item1.SchematicName];

                return Task.FromResult(machineRecord);
            }
            catch
            {
                throw new SchematicDoesNotExistException();
            }
        }

        public Task<Schematic<TState, TInput>> CreateSchematicAsync(Schematic<TState, TInput> schematic, string forkedFrom, CancellationToken cancellationToken)
        {
            Schematics.Add(schematic.SchematicName, schematic);

            var machineRecord = Schematics[schematic.SchematicName];

            return Task.FromResult(machineRecord);
        }

        public Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken)
        {
            return CreateSchematicAsync(schematic, null, cancellationToken);
        }

        public Task CreateMachineAsync(string schematicName, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var schematic = Schematics[schematicName];

            Machines.Add(machineId, (new MachineStatus<TState>
            {
                SchematicName = schematicName,
                State = schematic.InitialState,
                CommitTag = Guid.NewGuid(),
                StateChangedDateTime = DateTime.UtcNow
            }, metadata));

            return Task.CompletedTask;
        }

        public Task CreateMachineAsync(Schematic<TState, TInput> schematic, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            Schematics.Add(schematic.SchematicName, schematic);

            Machines.Add(machineId, (new MachineStatus<TState>
            {
                SchematicName = schematic.SchematicName,
                State = schematic.InitialState,
                CommitTag = Guid.NewGuid(),
                StateChangedDateTime = DateTime.UtcNow
            }, metadata));

            return Task.CompletedTask;
        }

        public Task CreateMachineAsync(string schematicName, string machineId, CancellationToken cancellationToken)
        {
            return CreateMachineAsync(schematicName, machineId, new Dictionary<string, string>(), cancellationToken);
        }

        public Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            Machines.Remove(machineId);

            return Task.CompletedTask;
        }

        public Task<State<TState>> GetMachineStateAsync(string machineId, CancellationToken cancellationToken)
        {
            var (machine, _) = Machines[machineId];

            return Task.FromResult<State<TState>>(machine);
        }

        public Task<State<TState>> SetMachineStateAsync(string machineId, TState state, TInput input, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            return SetMachineStateAsync(machineId, state, input, null, lastCommitTag, cancellationToken);
        }

        public Task<State<TState>> SetMachineStateAsync(string machineId, TState state, TInput input, string parameterData, Guid? lastCommitTag, CancellationToken cancellationToken)
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

            return Task.FromResult<State<TState>>(machine);
        }

        public Task<IDictionary<string, string>> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken)
        {
            var (_, metadata) = Machines[machineId];

            return Task.FromResult(metadata);
        }
    }
}
