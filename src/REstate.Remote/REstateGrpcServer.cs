using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Server;
using REstate.Remote.Services;

namespace REstate.Remote
{
    public class REstateGrpcServer
    {
        private Server Server { get; }

        public REstateGrpcServer(params ServerPort[] bindings)
        {
            var service = MagicOnionEngine.BuildServerServiceDefinition(
                targetTypes: new[]
                {
                    typeof(StateMachineService)
                },
                option: new MagicOnionOptions(
                    isReturnExceptionStackTraceInErrorDetail: true));

            Server = new Server
            {
                Services = { service }
            };

            foreach (var serverPort in bindings)
            {
                Server.Ports.Add(serverPort);
            }
        }

        public static void AddREstateToExisting(Server server)
        {
            var service = MagicOnionEngine.BuildServerServiceDefinition(
                targetTypes: new[]
                {
                    typeof(StateMachineService)
                },
                option: new MagicOnionOptions(
                    isReturnExceptionStackTraceInErrorDetail: true));

            server.Services.Add(service);
        }

        public Task StartAsync()
        {
            Server.Start();

            return Server.ShutdownTask;
        }

        public void Start()
        {
            Server.Start();
        }

        public Task ShutdownAsync()
        {
            return Server.ShutdownAsync();
        }

        public Task KillAsync()
        {
            return Server.KillAsync();
        }
    }
}