using System;
using System.Threading;
using REstate;
using Grpc.Core;
using MagicOnion.Server;
using System.Threading.Tasks;
using MagicOnion.Client;
using REstate.Interop;
using REstate.Interop.Services;

namespace Scratchpad
{
    class Program
    {
        private static async void ClientImpl()
        {
            // standard gRPC channel
            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);

            // create MagicOnion dynamic client proxy
            var client = MagicOnionClient.Create<IStateMachineService>(channel);

            var grpcStateEngine = new GrpcStateEngine<string, string>(client);

            var schematic = REstateHost
                .CreateSchematic<string, string>("EchoMachine")

                .WithState("Ready", state => state
                    .AsInitialState()
                    .WithOnEntry("Console", onEntry => onEntry
                        .DescribedAs("Echoes the payload to the console.")
                        .WithSetting("Format", "{2}")
                        .OnFailureSend("EchoFailure"))
                    .WithReentrance("Echo")//, transition => transition
                        //.WithGuard("Console", guard => guard
                            //.DescribedAs("Verfies action OK to take with y/n from console.")
                            //.WithSetting("Prompt", "Are you sure you want to echo \"{3}\"? (y/n)")))
                    )

                .WithState("EchoFailure", state => state
                    .AsSubStateOf("Ready")
                    .DescribedAs("An echo command failed to execute.")
                    .WithTransitionFrom("Ready", "EchoFailure"))

                .ToSchematic();

            var diagram = schematic.GetDiagram();

            var newSchematic = await grpcStateEngine.StoreSchematicAsync(schematic, CancellationToken.None);

            var echoMachineId = await REstateHost.GetStateEngine<string, string>()
                .CreateMachineAsync("EchoMachine", null, CancellationToken.None);

            var echoMachine = await grpcStateEngine.GetMachineAsync(echoMachineId, CancellationToken.None);

            var status = await echoMachine.SendAsync("Echo", "Hello!", CancellationToken.None);
        }

        private static void Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

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
            Task.Run(() =>
            {
                ClientImpl();
            });

            Console.ReadLine();
        }
    }
}