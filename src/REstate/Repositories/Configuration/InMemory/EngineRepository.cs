using REstate.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.InMemory
{
    public class EngineRepository: ISchematicRepository, IMachineRepository
    {
        private readonly StringSerializer _stringSerializer;
        private static IDictionary<string, Schematic> Schematics { get; } = new Dictionary<string, Schematic>();
        private static IDictionary<string, ValueTuple<Machine, IDictionary<string, string>>> Machines { get; } = new Dictionary<string, ValueTuple<Machine, IDictionary<string, string>>>();

        public EngineRepository(StringSerializer stringSerializer)
        {
            _stringSerializer = stringSerializer;
        }

        public Task<IEnumerable<Schematic>> ListSchematicsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Schematics.Values as IEnumerable<Schematic>);
        }

        public Task<Schematic> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken)
        {
            var machineRecord = Schematics[schematicName];

            return Task.FromResult(machineRecord);
        }

        public Task<Schematic> RetrieveSchematicForMachineAsync(string machineId, CancellationToken cancellationToken)
        {
            ValueTuple<Machine, IDictionary<string, string>> instanceData;
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

        public Task<Schematic> CreateSchematicAsync(Schematic schematic, string forkedFrom, CancellationToken cancellationToken)
        {
            Schematics.Add(schematic.SchematicName, schematic);

            var machineRecord = Schematics[schematic.SchematicName];

            return Task.FromResult(machineRecord);
        }

        public Task<Schematic> StoreSchematicAsync(Schematic schematic, CancellationToken cancellationToken)
        {
            return CreateSchematicAsync(schematic, null, cancellationToken);
        }

        public Task CreateMachineAsync(string schematicName, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            var schematic = Schematics[schematicName];

            Machines.Add(machineId, (new Machine
            {
                SchematicName = schematicName,
                StateName = schematic.InitialState,
                CommitTag = Guid.NewGuid().ToString(),
                StateChangedDateTime = DateTime.UtcNow
            }, metadata));

            return Task.CompletedTask;
        }

        public Task CreateMachineAsync(Schematic schematic, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            Schematics.Add(schematic.SchematicName, schematic);

            Machines.Add(machineId, (new Machine
            {
                SchematicName = schematic.SchematicName,
                StateName = schematic.InitialState,
                CommitTag = Guid.NewGuid().ToString(),
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

        public Task<Machine> GetMachineStateAsync(string machineId, CancellationToken cancellationToken)
        {
            var (machine, metadata) = Machines[machineId];

            return Task.FromResult(machine);
        }

        public Task<Machine> SetMachineStateAsync(string machineId, string stateName, string triggerName, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            return SetMachineStateAsync(machineId, stateName, triggerName, null, lastCommitTag, cancellationToken);
        }

        public Task<Machine> SetMachineStateAsync(string machineId, string stateName, string triggerName, string parameterData, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            var (machine, metadata) = Machines[machineId];

            lock(machine)
            {
                if (lastCommitTag == null || Guid.Parse(machine.CommitTag) == lastCommitTag)
                {
                    machine.StateName = stateName;
                    machine.CommitTag = Guid.NewGuid().ToString();
                }
                else
                {
                    throw new StateConflictException("CommitTag did not match.");
                }
            }

            return Task.FromResult(machine);
        }

        public Task<IDictionary<string, string>> GetMachineMetadataAsync(string machineId, CancellationToken cancellationToken)
        {
            var (machine, metadata) = Machines[machineId];

            return Task.FromResult(metadata);
        }
    }
}
