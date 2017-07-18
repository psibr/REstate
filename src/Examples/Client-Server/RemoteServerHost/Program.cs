using System;
using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion.Server;
using REstate.Remote.Services;

namespace RemoteServerHost
{
    class Program
    {
        static void Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var service = MagicOnionEngine
                .BuildServerServiceDefinition(
                    targetTypes: new[]
                    {
                        typeof(StateMachineService)
                    },
                    option: new MagicOnionOptions(
                        isReturnExceptionStackTraceInErrorDetail: true));

            var server = new Server
            {
                Services = { service },
                Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
            };

            // launch gRPC Server.
            server.Start();
        }
    }
}