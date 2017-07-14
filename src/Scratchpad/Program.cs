using System;
using System.Threading;
using REstate;
using Grpc.Core;
using MagicOnion.Server;
using System.Threading.Tasks;
using Grpc.Core.Logging;
using REstate.Remote;
using REstate.Remote.Services;

namespace Scratchpad
{
    class Program
    {
        private static async Task ClientImpl()
        {
            REstateHost.Agent.Configuration
                .RegisterComponent(
                    new GrpcRemoteHostComponent(
                        new GrpcHostOptions
                        {
                            Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                            UseAsDefaultEngine = true
                        }));

            var stateEngine = REstateHost.Agent
                .AsRemote()
                .GetStateEngine<string, string>();

            var schematic = REstateHost.Agent
                .CreateSchematic<string, string>("EchoMachine")

                .WithState("Ready", state => state
                    .AsInitialState()
                    .WithOnEntry("Console", onEntry => onEntry
                        .DescribedAs("Echoes the payload to the console.")
                        .WithSetting("Format", "{2}")
                        .OnFailureSend("EchoFailure"))
                    .WithReentrance("Echo", transition => transition
                        .WithGuard("Console", guard => guard
                            .DescribedAs("Verfies action OK to take with y/n from console.")
                            .WithSetting("Prompt", "Are you sure you want to echo \"{3}\"? (y/n)"))))

                .WithState("EchoFailure", state => state
                    .AsSubStateOf("Ready")
                    .DescribedAs("An echo command failed to execute.")
                    .WithTransitionFrom("Ready", "EchoFailure"))

                .ToSchematic();

            var diagram = schematic.WriteStateMap();

            var newSchematic = await stateEngine.StoreSchematicAsync(schematic, CancellationToken.None);

            var echoMachine = await REstateHost.Agent
                .AsLocal()
                .GetStateEngine<string, string>()
                .CreateMachineAsync("EchoMachine", null, CancellationToken.None);

            var status = await echoMachine.SendAsync("Echo", "Hello!", CancellationToken.None);
        }

        private static void Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var service = MagicOnionEngine.BuildServerServiceDefinition(
                targetTypes: new[]
                {
                    typeof(StateMachineService)
                },
                option: new MagicOnionOptions(isReturnExceptionStackTraceInErrorDetail: true));

            var server = new Server
            {
                Services = { service },
                Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
            };

            // launch gRPC Server.
            server.Start();

            // sample, launch server/client in same app.
            Task.Run(async () =>
            {
                await ClientImpl();
            }).Wait();

            Console.ReadLine();
        }
    }

    public class REstateRemoteHost
    {
    }
}