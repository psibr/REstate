using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    internal class MachineStatusStore<TState, TInput>
        : IMachineStatusStore<TState, TInput>
        , IOptimisticallyConcurrentMachineStatusStore<TState, TInput>
    {
        private readonly REstateDbContextFactory _dbContextFactory;
        private readonly string _machineId;

        public MachineStatusStore(REstateDbContextFactory dbContextFactory, string machineId)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _machineId = machineId ?? throw new ArgumentNullException(nameof(machineId));
        }

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> GetMachineStatusAsync(
            CancellationToken cancellationToken = default)
        {
            Machine machineRecord;

            using (var context = _dbContextFactory.CreateContext())
            {
                machineRecord = await context.Machines
                   .Include(machine => machine.MetadataEntries)
                   .Include(machine => machine.StateBagEntries)
                   .SingleOrDefaultAsync(
                       status => status.MachineId == _machineId,
                       cancellationToken).ConfigureAwait(false);
            }

            if (machineRecord == null) throw new MachineDoesNotExistException(_machineId);

            var schematic = machineRecord.SchematicBytes.ToSchematic<TState, TInput>();

            var state = machineRecord.StateJson.ToState<TState>();

            var metadata = machineRecord.MetadataEntries
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            var stateBag = machineRecord.StateBagEntries
                .ToDictionary(entry => entry.Key, entry => entry.Value);

            return new MachineStatus<TState, TInput>
            {
                MachineId = _machineId,
                Schematic = schematic,
                State = state,
                Metadata = metadata,
                CommitNumber = machineRecord.CommitNumber,
                UpdatedTime = machineRecord.UpdatedTime,
                StateBag = stateBag
            };
        }

        async Task<MachineStatus<TState, TInput>> IOptimisticallyConcurrentMachineStatusStore<TState, TInput>.SetMachineStateAsync(
                MachineStatus<TState, TInput> machineStatus,
                TState state,
                long? lastCommitNumber,
                IDictionary<string, string> stateBag,
                CancellationToken cancellationToken)
        {
            if (machineStatus == null) throw new ArgumentNullException(nameof(machineStatus));
            if (machineStatus.MachineId == null) throw new ArgumentException("MachineId must be provided", nameof(machineStatus));
            if (machineStatus.MachineId != _machineId) throw new ArgumentException("MachineId must match", nameof(machineStatus));

            var machineRecord = new Machine
            {
                MachineId = machineStatus.MachineId,
                CommitNumber = machineStatus.CommitNumber
            };

            var newStatus = new MachineStatus<TState, TInput>
            {
                MachineId = machineStatus.MachineId,
                Schematic = machineStatus.Schematic,
                State = state,
                Metadata = machineStatus.Metadata
            };

            using (var context = _dbContextFactory.CreateContext())
            {

                if (lastCommitNumber == null || machineStatus.CommitNumber == lastCommitNumber)
                {
                    context.Machines.Attach(machineRecord);

                    var stateJson = state.ToStateRepresentation();

                    machineRecord.StateJson = stateJson;
                    machineRecord.CommitNumber++;
                    machineRecord.UpdatedTime = DateTimeOffset.UtcNow;

                    if (stateBag != null && lastCommitNumber != null)
                    {
                        var inserts = stateBag.Keys.Except(machineStatus.StateBag.Keys).Select(k => new StateBagEntry { Key = k, MachineId = machineStatus.MachineId, Value = stateBag[k] });
                        var updates = machineStatus.StateBag.Keys.Intersect(stateBag.Keys).Select(k => new StateBagEntry { Key = k, MachineId = machineStatus.MachineId }).ToList();
                        var deletes = machineStatus.StateBag.Keys.Except(stateBag.Keys).Select(k => new StateBagEntry { Key = k, MachineId = machineStatus.MachineId }).ToList();

                        context.StateBagEntries.AddRange(inserts);

                        context.AttachRange(updates);

                        foreach (var stateBagEntry in updates)
                        {
                            stateBagEntry.Value = stateBag[stateBagEntry.Key];
                        }

                        context.StateBagEntries.AttachRange(deletes);
                        context.StateBagEntries.RemoveRange(deletes);

                        newStatus.StateBag = stateBag;
                    }
                    else
                    {
                        newStatus.StateBag = machineStatus.StateBag;
                    }
                }
                else
                {
                    throw new StateConflictException();
                }

                try
                {
                    await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new StateConflictException(ex);
                }
            }

            newStatus.CommitNumber = machineRecord.CommitNumber;
            newStatus.UpdatedTime = machineRecord.UpdatedTime;

            return newStatus;
        }

        /// <inheritdoc />
        public async Task<MachineStatus<TState, TInput>> SetMachineStateAsync(
            TState state,
            long? lastCommitNumber,
            IDictionary<string, string> stateBag = null,
            CancellationToken cancellationToken = default)
        {
            Machine machineRecord;
            using (var context = _dbContextFactory.CreateContext())
            {
                machineRecord = await context.Machines
                    .Include(machine => machine.MetadataEntries)
                    .Include(machine => machine.StateBagEntries)
                    .SingleOrDefaultAsync(
                        status => status.MachineId == _machineId,
                        cancellationToken).ConfigureAwait(false);
            }

            if (machineRecord == null) throw new MachineDoesNotExistException(_machineId);

            var schematic = machineRecord.SchematicBytes.ToSchematic<TState, TInput>();

            var metadata = machineRecord.MetadataEntries?.ToDictionary(entry => entry.Key, entry => entry.Value);

            var currentStateBag = machineRecord.StateBagEntries?.ToDictionary(entry => entry.Key, entry => entry.Value);

            var machineStatus = new MachineStatus<TState, TInput>
            {
                MachineId = _machineId,
                Schematic = schematic,
                State = state,
                Metadata = metadata,
                CommitNumber = machineRecord.CommitNumber,
                UpdatedTime = machineRecord.UpdatedTime,
                StateBag = currentStateBag
            };

            return await (this as IOptimisticallyConcurrentMachineStatusStore<TState, TInput>)
                .SetMachineStateAsync(machineStatus, state, lastCommitNumber, stateBag, cancellationToken)
                .ConfigureAwait(false);
        }
    }

}
