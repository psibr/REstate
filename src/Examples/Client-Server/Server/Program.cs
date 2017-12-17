using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using REstate;
using REstate.Engine.Repositories.Redis;
using Serilog;
using Server.Configuration.REstate;

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger = logger;

            var serverShutdownCancellationSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) => 
                serverShutdownCancellationSource.Cancel();

            var configuration = new ConfigurationBuilder()
                .Add(new MemoryConfigurationSource
                {
                    InitialData = new Dictionary<string, string>
                    {
                        ["REstate:Server:Binding:Address"] = "0.0.0.0",
                        ["REstate:Server:Binding:Port"] = "12345"
                    }
                })
                .AddEnvironmentVariables()
                .Build();

            var serverConfiguration = configuration
                .GetSection("REstate:Server")
                .Get<ServerConfiguration>();

            //var redisMultiplexer = await StackExchange.Redis.ConnectionMultiplexer
            //    .ConnectAsync(serverConfiguration.RepositoryConnectionString);

            //REstateHost.Agent.Configuration
            //    .RegisterComponent(
            //        component: new RedisRepositoryComponent(
            //            restateDatabase: redisMultiplexer.GetDatabase()));

            var server = REstateHost.Agent
                .AsRemote()
                .CreateGrpcServer(new ServerPort(
                    host: serverConfiguration.Binding.Address,
                    port: serverConfiguration.Binding.Port,
                    credentials: ServerCredentials.Insecure));

            logger.Information("Starting REstate gRPC server.");

            serverShutdownCancellationSource.Token.Register(() =>
                server.ShutdownAsync().GetAwaiter().GetResult());

            while (true)
            {
                await server.StartAsync();

                if (server.ShutdownTask.IsFaulted)
                {
                    logger.Error("Server ran into a fault, restarting.");

                    continue;
                }

                break;
            }

            logger.Information("Shutdown of REstate gRPC server completed.");
        }
    }
}