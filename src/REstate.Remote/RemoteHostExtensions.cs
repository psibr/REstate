using Grpc.Core;
using REstate.Remote;

namespace REstate
{
    public static class RemoteHostExtensions
    {
        public static IRemoteAgent AsRemote(this IAgent agent) =>
            new RemoteAgent(agent);

        public static REstateGrpcServer CreateGrpcServer(this IRemoteAgent remoteAgent, params ServerPort[] bindings) => 
            new REstateGrpcServer(bindings);
    }
}
