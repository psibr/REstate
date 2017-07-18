using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using MessagePack;
using MessagePack.Resolvers;
using REstate.Engine;
using REstate.Remote.Models;
using REstate.Remote.Services;
using REstate.Schematics;

namespace REstate.Remote
{
    public class GrpcStateMachine<TState, TInput>
        : IStateMachine<TState, TInput>
    {
        private readonly IStateMachineService _stateMachineService;

        public GrpcStateMachine(IStateMachineService stateMachineService, string machineId)
        {
            _stateMachineService = stateMachineService;

            MachineId = machineId;
        }

        public async Task<ISchematic<TState, TInput>> GetSchematicAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .GetMachineSchematicAsync(new GetMachineSchematicRequest
                {
                    MachineId = MachineId
                });

            return MessagePackSerializer
                .Deserialize<Schematic<TState, TInput>>(
                    response.SchematicBytes,
                    ContractlessStandardResolver.Instance);
        }

        public string MachineId { get; }

        public async Task<State<TState>> SendAsync<TPayload>(TInput input, TPayload payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            SendResponse response;

            try
            {
                response = await _stateMachineService
                    .WithCancellationToken(cancellationToken)
                    .SendWithPayloadAsync(new SendWithPayloadRequest
                    {
                        MachineId = MachineId,
                        InputBytes = MessagePackSerializer.Serialize(input, ContractlessStandardResolver.Instance),
                        PayloadBytes = MessagePackSerializer.Typeless.Serialize(payload)
                    });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.AlreadyExists)
                    throw new StateConflictException(exception.Status.Detail, exception);

                throw;
            }

            return new State<TState>(
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    ContractlessStandardResolver.Instance),
                response.CommitTag);
        }

        public async Task<State<TState>> SendAsync<TPayload>(TInput input, TPayload payload, Guid lastCommitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            SendResponse response;

            try
            {
                response = await _stateMachineService
                    .WithCancellationToken(cancellationToken)
                    .SendWithPayloadAsync(new SendWithPayloadRequest
                    {
                        MachineId = MachineId,
                        InputBytes = MessagePackSerializer.Serialize(input, ContractlessStandardResolver.Instance),
                        PayloadBytes = MessagePackSerializer.Typeless.Serialize(payload),
                        CommitTag = lastCommitTag
                    });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.AlreadyExists)
                    throw new StateConflictException(exception.Status.Detail, exception);

                throw;
            }

            return new State<TState>(
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    ContractlessStandardResolver.Instance),
                response.CommitTag);
        }

        public async Task<State<TState>> SendAsync(TInput input, CancellationToken cancellationToken = default(CancellationToken))
        {
            SendResponse response;

            try
            {
                response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .SendAsync(new SendRequest
                {
                    MachineId = MachineId,
                    InputBytes = MessagePackSerializer.Serialize(input, ContractlessStandardResolver.Instance)
                });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.AlreadyExists)
                    throw new StateConflictException(exception.Status.Detail, exception);

                throw;
            }

            return new State<TState>(
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    ContractlessStandardResolver.Instance),
                response.CommitTag);
        }

        public async Task<State<TState>> SendAsync(TInput input, Guid lastCommitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            SendResponse response;

            try
            {
                response = await _stateMachineService
                    .WithCancellationToken(cancellationToken)
                    .SendAsync(new SendRequest
                    {
                        MachineId = MachineId,
                        InputBytes = MessagePackSerializer.Serialize(input, ContractlessStandardResolver.Instance),
                        CommitTag = lastCommitTag
                    });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.AlreadyExists)
                    throw new StateConflictException(exception.Status.Detail, exception);

                throw;
            }

            return new State<TState>(
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    ContractlessStandardResolver.Instance),
                response.CommitTag);
        }

        // TODO: Promote to the StateVisor system.
        public Task<bool> IsInStateAsync(State<TState> state, CancellationToken cancellationToken = default(CancellationToken))
        {
            return null;
        }

        // TODO: Promote to the StateVisor system.
        public Task<State<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return null;
        }

        // TODO: Move to a lookup extension method on ISchematic given a state.
        public Task<ICollection<TInput>> GetPermittedInputAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return null;
        }
    }
}