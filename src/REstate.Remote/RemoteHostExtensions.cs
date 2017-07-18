using Grpc.Core;
using REstate.Remote;

namespace REstate
{
    public static class RemoteHostExtensions
    {
        public static IRemoteHost AsRemote(this IAgent agent) =>
            new RemoteHost(agent);

        public static REstateGrpcServer CreateGrpcServer(this IRemoteHost remoteHost, params ServerPort[] bindings) => 
            new REstateGrpcServer(bindings);
    }
}