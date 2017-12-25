﻿using System;
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
        private static IDictionary<string, Schematic<TState, TInput>> Schematics { get; } = 
            new Dictionary<string, Schematic<TState, TInput>>();
        private static IDictionary<string, (MachineStatus<TState, TInput> MachineStatus, Metadata Metadata)> Machines { get; } =
            new Dictionary<string, (MachineStatus<TState, TInput>, Metadata)>();

        public Task<Schematic<TState, TInput>> RetrieveSchematicAsync(
            string schematicName, 
            CancellationToken cancellationToken = default)
        {
            var schematic = Schematics[schematicName];

            return Task.FromResult(schematic);
        }

        public Task<Schematic<TState, TInput>> StoreSchematicAsync(
            Schematic<TState, TInput> schematic, 
            CancellationToken cancellationToken = default)
        {
            Schematics.Add(schematic.SchematicName, schematic);

            var storedSchematic = Schematics[schematic.SchematicName];

            return Task.FromResult(storedSchematic);
        }

        public Task<MachineStatus<TState, TInput>> CreateMachineAsync(
            string schematicName,
            string machineId,
            Metadata metadata, 
            CancellationToken cancellationToken = default)
        {
            var schematic = Schematics[schematicName];

            return CreateMachineAsync(schematic, machineId, metadata, cancellationToken);
        }

        public Task<MachineStatus<TState, TInput>> CreateMachineAsync(
            Schematic<TState, TInput> schematic,
            string machineId,
            Metadata metadata, 
            CancellationToken cancellationToken = default)
        {
            var id = machineId ?? Guid.NewGuid().ToString();

            var record = new MachineStatus<TState, TInput>
            {
                MachineId = id,
                Schematic = schematic,
                State = schematic.InitialState,
                CommitTag = Guid.NewGuid(),
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
                            CommitTag = Guid.NewGuid(),
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

        public Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            Machines.Remove(machineId);

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
            var (machine, _) = Machines[machineId];

            return Task.FromResult(machine);
        }

        public Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            string machineId,
            TState state,
            Guid? lastCommitTag,
            CancellationToken cancellationToken = default)
        {
            var (machine, _) = Machines[machineId];

            lock(machine)
            {
                if (lastCommitTag == null || machine.CommitTag == lastCommitTag)
                {
                    machine.State = state;
                    machine.CommitTag = Guid.NewGuid();
                    machine.UpdatedTime = DateTimeOffset.UtcNow;
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
