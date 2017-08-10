using System;
using System.Collections.Generic;
using System.Threading;
using REstate;
using Grpc.Core;
using System.Threading.Tasks;
using Grpc.Core.Logging;
using REstate.Engine;
using REstate.Engine.Repositories.Redis;
using REstate.Remote;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using StackExchange.Redis;

namespace Scratchpad
{
    class Program
    {
        private static async Task ClientImpl()
        {
            var visor = new InMemoryStateVisor();

            REstateHost.Agent.Configuration
                .RegisterComponent(new InMemoryStateVisorComponent(visor));

            var connection = await ConnectionMultiplexer.ConnectAsync(
                "restate.redis.cache.windows.net:6380," +
                "password=m7+cQKqV3x2KQaAOned8xGj62/0E2t0DrLk/KEElfH0=," +
                "ssl=True," +
                "abortConnect=False");

            REstateHost.Agent.Configuration
                .RegisterComponent(new RedisRepositoryComponent(connection.GetDatabase()));

            REstateHost.Agent.Configuration
                .RegisterComponent(
                    new GrpcRemoteHostComponent(
                        new GrpcHostOptions
                        {
                            Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                            UseAsDefaultEngine = true
                        }));

            var stateEngine = REstateHost.Agent
                .GetStateEngine<string, string>();

            var schematic = REstateHost.Agent
                .CreateSchematic<string, string>("EchoMachine")
                
                .WithState("Created", state => state
                    .DescribedAs("EchoMachine was created, but no echo has occured.")
                    .AsInitialState())

                .WithState("Ready", state => state
                    .DescribedAs("EchoMachine has received an echo command, and is ready to echo again.")
                    .WithTransitionFrom("Created", "Echo", transition => transition
                        .WithGuard("Console", guard => guard
                            .DescribedAs("Verfies action OK to take with y/n from console.")
                            .WithSetting("Prompt", "Are you sure you want to echo \"{3}\"? (y/n)")))
                    .WithOnEntry("Log", onEntry => onEntry
                        .DescribedAs("Echoes the payload to the console.")
                        .WithSetting("message", "{3}")
                        .OnFailureSend("EchoFailure"))
                    .WithReentrance("Echo", transition => transition
                        .WithGuard("Console", guard => guard
                            .DescribedAs("Verfies action OK to take with y/n from console.")
                            .WithSetting("Prompt", "Are you sure you want to echo \"{3}\"? (y/n)"))))

                .WithState("EchoFailure", state => state
                    .DescribedAs("An echo command failed to execute.")
                    .WithTransitionFrom("Ready", "EchoFailure")
                    .WithOnEntry("Log", onEntry => onEntry
                        .DescribedAs("Echoes the payload to the console.")
                        .WithSetting("severity", "fatal")))

                .Build();
            
            var newSchematic = await stateEngine.StoreSchematicAsync(schematic, CancellationToken.None);

            await stateEngine.BulkCreateMachinesAsync("EchoMachine", new IDictionary<string, string>[350000],
                CancellationToken.None);

            var echoMachine = await stateEngine.CreateMachineAsync("EchoMachine", null, CancellationToken.None);

            await stateEngine.DeleteMachineAsync(echoMachine.MachineId, CancellationToken.None);

            echoMachine = await stateEngine.CreateMachineAsync(schematic, null, CancellationToken.None);

            echoMachine = await stateEngine.GetMachineAsync(echoMachine.MachineId, CancellationToken.None);

            var machineSchematic = await echoMachine.GetSchematicAsync(CancellationToken.None);

            var status = await echoMachine.SendAsync("Echo", "Hello!", CancellationToken.None);

            var commitTag = status.CommitTag;

            status = await echoMachine.SendAsync("Echo", CancellationToken.None);

            try
            {
                status = await echoMachine.SendAsync("Echo", "Fail", commitTag, CancellationToken.None);
            }
            catch(StateConflictException ex)
            {
                Log.Logger.Error(ex, "State conflict occured.");
            }

            Console.WriteLine($"EchoMachine ended with state: { visor.GetStatus(echoMachine).State }.");

            await stateEngine.DeleteMachineAsync(echoMachine.MachineId, CancellationToken.None);
        }

        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            GrpcEnvironment.SetLogger(new ConsoleLogger());

            using (var server = REstateHost.Agent
                .AsRemote()
                .CreateGrpcServer(new ServerPort("localhost", 12345, ServerCredentials.Insecure)))
            {
                server.Start();

                // sample, launch server/client in same app.
                Task.Run(async () =>
                {
                    await ClientImpl();
                }).Wait();

                Console.ReadLine();
            }
        }
    }
}