using System;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Server;
using REstate.Remote.Services;

namespace REstate.Remote
{
    public class REstateGrpcServer 
        : IDisposable
    {
        private Server Server { get; set; }

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

        public Task ShutdownTask => Server.ShutdownTask;

        public Task StartAsync()
        {
            Server.Start();
            return Server.ShutdownTask;
        }

        public void Start()
        {
            try
            {
                Server.Start();
            }
            catch (InvalidOperationException)
            {
                // Swallow server already started exceptions
            }
        }

        public Task ShutdownAsync()
        {
            return Server.ShutdownAsync();
        }

        public Task KillAsync()
        {
            return Server.KillAsync();
        }

        /// <summary>
        /// Performs application-defined tasks associated with 
        /// freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(!ShutdownTask.IsCompleted)
                ShutdownAsync().GetAwaiter().GetResult();

            Server = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows an object to try to free resources and perform other 
        /// cleanup operations before it is reclaimed by garbage collection.
        /// </summary>
        ~REstateGrpcServer()
        {
            KillAsync().GetAwaiter().GetResult();
        }
    }
}