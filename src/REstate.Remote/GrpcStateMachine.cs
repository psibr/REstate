using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
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
            var response = await _stateMachineService.GetMachineSchematicAsync(new GetMachineSchematicRequest
            {
                MachineId = MachineId
            });

            return MessagePackSerializer
                .Deserialize<Schematic<TState, TInput>>(response.SchematicBytes,
                    MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        public string MachineId { get; }

        public async Task<State<TState>> SendAsync<TPayload>(TInput input, TPayload payload, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _stateMachineService.SendAsync(new SendRequest
            {
                MachineId = MachineId,
                InputBytes = MessagePackSerializer.Serialize(input, MessagePack.Resolvers.ContractlessStandardResolver.Instance),
                PayloadBytes = MessagePackSerializer.Typeless.Serialize(payload),
            });

            return new State<TState>(MessagePackSerializer.Deserialize<TState>(response.StateBytes, MessagePack.Resolvers.ContractlessStandardResolver.Instance),
                response.CommitTag);
        }

        public Task<State<TState>> SendAsync<TPayload>(TInput input, TPayload payload, Guid? lastCommitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            return null;
        }

        public Task<State<TState>> SendAsync(TInput input, CancellationToken cancellationToken = default(CancellationToken))
        {
            return null;
        }

        public Task<State<TState>> SendAsync(TInput input, Guid? lastCommitTag, CancellationToken cancellationToken = default(CancellationToken))
        {
            return null;
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