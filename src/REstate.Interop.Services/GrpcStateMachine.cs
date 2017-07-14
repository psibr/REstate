using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using REstate.Remote.Models;
using REstate.Remote.Services;

namespace REstate.Remote
{
    public class GrpcStateMachine<TState, TInput>
        : IStateMachine<TState, TInput>
    {
        private readonly IStateMachineService _stateMachineService;

        public GrpcStateMachine(IStateMachineService stateMachineService, string machineId)
        {
            MachineId = machineId;
            _stateMachineService = stateMachineService;
        }

        public string MachineId { get; }

        public async Task<State<TState>> SendAsync<TPayload>(TInput input, TPayload payload, CancellationToken cancellationToken)
        {
            var response = await _stateMachineService.SendAsync(new SendRequest
            {
                MachineId = MachineId,
                InputBytes = MessagePackSerializer.Serialize(input, MessagePack.Resolvers.ContractlessStandardResolver.Instance),
                PayloadBytes = MessagePackSerializer.Typeless.Serialize(payload),
            });

            return new State<TState>(MessagePackSerializer.Deserialize<TState>((byte[]) response.StateBytes, MessagePack.Resolvers.ContractlessStandardResolver.Instance),
                response.CommitTag);
        }

        public Task<State<TState>> SendAsync<TPayload>(TInput input, TPayload payload, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<State<TState>> SendAsync(TInput input, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<State<TState>> SendAsync(TInput input, Guid? lastCommitTag, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<bool> IsInStateAsync(State<TState> state, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<State<TState>> GetCurrentStateAsync(CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<ICollection<TInput>> GetPermittedInputAsync(CancellationToken cancellationToken)
        {
            return null;
        }
    }
}