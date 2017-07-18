using System;
using Grpc.Core;
using Grpc.Core.Logging;
using MagicOnion.Server;
using REstate;
using REstate.Remote.Services;

namespace RemoteServerHost
{
    class Program
    {
        static void Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var server = REstateHost.Agent
                .AsRemote()
                .CreateGrpcServer(new ServerPort("localhost", 12345, ServerCredentials.Insecure));

            // launch gRPC Server.
            server.Start();

            Console.ReadLine();
        }
    }
}