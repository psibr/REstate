using REstate.Remote;

namespace REstate
{
    public static class RemoteHostExtensions
    {
        public static IRemoteHost AsRemote(this IAgent agent) =>
            new RemoteHost(agent);
    }
}