using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ISchematic<TState, TInput> _machineSchematic;

        private IReadOnlyDictionary<string, string> _metadata;

        public GrpcStateMachine(IStateMachineService stateMachineService, string machineId)
        {
            _stateMachineService = stateMachineService;

            MachineId = machineId;
        }

        public string MachineId { get; }

        public async Task<ISchematic<TState, TInput>> GetSchematicAsync(CancellationToken cancellationToken = default)
        {
            if (_machineSchematic != null)
                return _machineSchematic;

            var response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .GetMachineSchematicAsync(new GetMachineSchematicRequest
                {
                    MachineId = MachineId
                });

            _machineSchematic = MessagePackSerializer
                .Deserialize<Schematic<TState, TInput>>(
                    response.SchematicBytes,
                    MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block));

            return _machineSchematic;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetMetadataAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (_metadata != null)
                return _metadata;

            var response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .GetMachineMetadataAsync(new GetMachineMetadataRequest
                {
                    MachineId = MachineId
                });

            _metadata = new ReadOnlyDictionary<string, string>(response.Metadata ?? new Dictionary<string, string>(0));

            return _metadata;
        }

        public async Task<Status<TState>> SendAsync<TPayload>(TInput input, TPayload payload, CancellationToken cancellationToken = default)
        {
            SendResponse response;

            try
            {
                response = await _stateMachineService
                    .WithCancellationToken(cancellationToken)
                    .SendWithPayloadAsync(new SendWithPayloadRequest
                    {
                        MachineId = MachineId,
                        InputBytes = MessagePackSerializer.Serialize(input, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                        PayloadBytes = MessagePackSerializer.Typeless.Serialize(payload, MessagePackSerializerOptions.Standard.WithResolver(TypelessContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block))
                    });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.OutOfRange)
                    throw new TransitionNotDefinedException(exception.Status.Detail, exception);
                if (exception.Status.StatusCode == StatusCode.FailedPrecondition)
                    throw new TransitionFailedPreconditionException(exception.Status.Detail, exception);
                if (exception.Status.StatusCode == StatusCode.AlreadyExists)
                    throw new StateConflictException(exception.Status.Detail, exception);
                if (exception.Status.StatusCode == StatusCode.NotFound)
                    throw new MachineDoesNotExistException(MachineId, exception.Status.Detail, exception);

                throw;
            }

            return new Status<TState>(
                MachineId,
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                response.UpdatedTime,
                response.CommitNumber,
                response.StateBag);
        }

        public async Task<Status<TState>> SendAsync<TPayload>(TInput input, TPayload payload, long lastCommitNumber, IDictionary<string, string> stateBag = null, CancellationToken cancellationToken = default)
        {
            SendResponse response;

            try
            {
                response = await _stateMachineService
                    .WithCancellationToken(cancellationToken)
                    .SendWithPayloadAsync(new SendWithPayloadRequest
                    {
                        MachineId = MachineId,
                        InputBytes = MessagePackSerializer.Serialize(input, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                        PayloadBytes = MessagePackSerializer.Typeless.Serialize(payload, MessagePackSerializerOptions.Standard.WithResolver(TypelessContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                        CommitNumber = lastCommitNumber,
                        StateBag = stateBag
                    });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.AlreadyExists)
                    throw new StateConflictException(exception.Status.Detail, exception);
                if (exception.Status.StatusCode == StatusCode.NotFound)
                    throw new MachineDoesNotExistException(MachineId, exception.Status.Detail, exception);

                throw;
            }

            return new Status<TState>(
                MachineId,
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                response.UpdatedTime,
                response.CommitNumber,
                response.StateBag);
        }

        public async Task<Status<TState>> SendAsync(TInput input, CancellationToken cancellationToken = default)
        {
            SendResponse response;

            try
            {
                response = await _stateMachineService
                .WithCancellationToken(cancellationToken)
                .SendAsync(new SendRequest
                {
                    MachineId = MachineId,
                    InputBytes = MessagePackSerializer.Serialize(input, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block))
                });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.OutOfRange)
                    throw new TransitionNotDefinedException(exception.Status.Detail, exception);
                if (exception.Status.StatusCode == StatusCode.FailedPrecondition)
                    throw new TransitionFailedPreconditionException(exception.Status.Detail, exception);
                if (exception.Status.StatusCode == StatusCode.AlreadyExists)
                    throw new StateConflictException(exception.Status.Detail, exception);
                if (exception.Status.StatusCode == StatusCode.NotFound)
                    throw new MachineDoesNotExistException(MachineId, exception.Status.Detail, exception);

                throw;
            }

            return new Status<TState>(
                MachineId,
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                response.UpdatedTime,
                response.CommitNumber,
                response.StateBag);
        }

        public async Task<Status<TState>> SendAsync(TInput input, long lastCommitNumber, IDictionary<string, string> stateBag = null, CancellationToken cancellationToken = default)
        {
            SendResponse response;

            try
            {
                response = await _stateMachineService
                    .WithCancellationToken(cancellationToken)
                    .SendAsync(new SendRequest
                    {
                        MachineId = MachineId,
                        InputBytes = MessagePackSerializer.Serialize(input, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                        CommitNumber = lastCommitNumber,
                        StateBag = stateBag
                    });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.AlreadyExists)
                    throw new StateConflictException(exception.Status.Detail, exception);
                if (exception.Status.StatusCode == StatusCode.NotFound)
                    throw new MachineDoesNotExistException(MachineId, exception.Status.Detail, exception);

                throw;
            }

            return new Status<TState>(
                MachineId,
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                response.UpdatedTime,
                response.CommitNumber,
                response.StateBag);
        }

        public async Task<Status<TState>> GetCurrentStateAsync(CancellationToken cancellationToken = default)
        {
            GetCurrentStateResponse response;

            try
            {
                response = await _stateMachineService
                    .WithCancellationToken(cancellationToken)
                    .GetCurrentStateAsync(new GetCurrentStateRequest
                    {
                        MachineId = MachineId
                    });
            }
            catch (RpcException exception)
            {
                if (exception.Status.StatusCode == StatusCode.NotFound)
                    throw new MachineDoesNotExistException(MachineId, exception.Status.Detail, exception);

                throw;
            }

            return new Status<TState>(
                MachineId,
                MessagePackSerializer.Deserialize<TState>(
                    response.StateBytes,
                    MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block)),
                response.UpdatedTime,
                response.CommitNumber,
                response.StateBag);
        }
    }
}
