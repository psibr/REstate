using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Engine;
using REstate.Remote.Models;
using REstate.Schematics;

namespace REstate.Remote.Services
{
    public class StateMachineServiceLocalAdapter
        : IStateMachineServiceLocalAdapter
    {
        public async Task<SendResponse> SendAsync<TState, TInput>(
            string machineId,
            TInput input,
            long? commitNumber,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent.AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);

            Status<TState> newStatus;

            try
            {
                newStatus = commitNumber != null
                    ? await machine.SendAsync(input, commitNumber.Value, cancellationToken).ConfigureAwait(false)
                    : await machine.SendAsync(input, cancellationToken).ConfigureAwait(false);
            }
            catch (StateConflictException conflictException)
            {
                throw new ReturnStatusException(StatusCode.AlreadyExists, conflictException.Message);
            }

            return new SendResponse
            {
                MachineId = machineId,
                CommitNumber = newStatus.CommitNumber,
                StateBytes = MessagePackSerializer.Serialize(newStatus.State, ContractlessStandardResolver.Instance),
                UpdatedTime = newStatus.UpdatedTime
            };
        }

        public async Task<SendResponse> SendWithPayloadAsync<TState, TInput, TPayload>(
            string machineId,
            TInput input,
            TPayload payload,
            long? commitNumber,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent.AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);

            Status<TState> newStatus;

            try
            {
                newStatus = commitNumber != null
                    ? await machine.SendAsync(input, payload, commitNumber.Value, cancellationToken).ConfigureAwait(false)
                    : await machine.SendAsync(input, payload, cancellationToken).ConfigureAwait(false);
            }
            catch (StateConflictException conflictException)
            {
                throw new ReturnStatusException(StatusCode.AlreadyExists, conflictException.Message);
            }

            return new SendResponse
            {
                MachineId = machineId,
                CommitNumber = newStatus.CommitNumber,
                StateBytes = MessagePackSerializer.Serialize(newStatus.State, ContractlessStandardResolver.Instance),
                UpdatedTime = newStatus.UpdatedTime
            };
        }

        public async Task<StoreSchematicResponse> StoreSchematicAsync<TState, TInput>(
            Schematic<TState, TInput> schematic,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent.AsLocal()
                .GetStateEngine<TState, TInput>();

            var newSchematic = await engine.StoreSchematicAsync(schematic, cancellationToken).ConfigureAwait(false);

            return new StoreSchematicResponse
            {
                SchematicBytes = MessagePackSerializer.NonGeneric.Serialize(
                    newSchematic.GetType(),
                    newSchematic,
                    ContractlessStandardResolver.Instance)
            };
        }

        public async Task<GetMachineSchematicResponse> GetMachineSchematicAsync<TState, TInput>(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            IStateMachine<TState, TInput> machine;
            try
            {
                machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);
            }
            catch (MachineDoesNotExistException machineDoesNotExistException)
            {
                throw new ReturnStatusException(StatusCode.NotFound, machineDoesNotExistException.Message);
            }

            var schematic = await machine.GetSchematicAsync(cancellationToken).ConfigureAwait(false);

            return new GetMachineSchematicResponse
            {
                MachineId = machine.MachineId,
                SchematicBytes = MessagePackSerializer.NonGeneric
                    .Serialize(schematic.GetType(), schematic, ContractlessStandardResolver.Instance)
            };
        }

        public async Task<GetMachineMetadataResponse> GetMachineMetadataAsync<TState, TInput>(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.GetMachineAsync(machineId, cancellationToken).ConfigureAwait(false);

            var metadata = await machine.GetMetadataAsync(cancellationToken).ConfigureAwait(false);

            return new GetMachineMetadataResponse
            {
                MachineId = machine.MachineId,
                Metadata = (IDictionary<string, string>)metadata
            };
        }

        public async Task<CreateMachineResponse> CreateMachineFromStoreAsync<TState, TInput>(
            string schematicName,
            string machineId,
            IDictionary<string, string> metadata,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.CreateMachineAsync(
                schematicName,
                machineId,
                metadata,
                cancellationToken);

            return new CreateMachineResponse
            {
                MachineId = machine.MachineId
            };
        }

        public async Task<CreateMachineResponse> CreateMachineFromSchematicAsync<TState, TInput>(
            Schematic<TState, TInput> schematic,
            string machineId,
            IDictionary<string, string> metadata,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machine = await engine.CreateMachineAsync(
                schematic,
                machineId,
                metadata,
                cancellationToken);

            return new CreateMachineResponse
            {
                MachineId = machine.MachineId
            };
        }

        public async Task<BulkCreateMachineResponse> BulkCreateMachineFromStoreAsync<TState, TInput>(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machines = await engine.BulkCreateMachinesAsync(schematicName, metadata, cancellationToken);

            return new BulkCreateMachineResponse
            {
                MachineIds = machines.Select(machine => machine.MachineId)
            };
        }

        public async Task<BulkCreateMachineResponse> BulkCreateMachineFromSchematicAsync<TState, TInput>(
            Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            var engine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var machines = await engine.BulkCreateMachinesAsync(schematic, metadata, cancellationToken);
            
            return new BulkCreateMachineResponse
            {
                MachineIds = machines.Select(machine => machine.MachineId)
            };
        }

        public async Task<GetSchematicResponse> GetSchematicAsync<TState, TInput>(
            string schematicName,
            CancellationToken cancellationToken)
        {
            var stateEngine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            var schematic = await stateEngine.GetSchematicAsync(
                schematicName,
                cancellationToken).ConfigureAwait(false);

            return new GetSchematicResponse
            {
                SchematicBytes = MessagePackSerializer.NonGeneric.Serialize(
                    schematic.GetType(),
                    schematic,
                    ContractlessStandardResolver.Instance)
            };
        }

        public async Task DeleteMachineAsync<TState, TInput>(
            string machineId,
            CancellationToken cancellationToken)
        {
            var stateEngine = REstateHost.Agent
                .AsLocal()
                .GetStateEngine<TState, TInput>();

            await stateEngine.DeleteMachineAsync(
                machineId,
                cancellationToken).ConfigureAwait(false);
        }
    }
}
