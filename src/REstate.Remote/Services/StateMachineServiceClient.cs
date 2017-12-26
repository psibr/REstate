using MagicOnion.Client;

namespace REstate.Remote.Services
{
    public class StateMachineServiceClient
        : IStateMachineServiceClient
    {
        private readonly GrpcHostOptions _gRpcHostOptions;

        public StateMachineServiceClient(GrpcHostOptions gRpcHostOptions)
        {
            _gRpcHostOptions = gRpcHostOptions;
        }

        public IStateMachineService Create() =>
            MagicOnionClient.Create<IStateMachineService>(_gRpcHostOptions.Channel);
    }
}