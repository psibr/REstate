using System;
using System.Threading;
using Grpc.Core;
using REstate;
using REstate.Remote;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            REstateHost.Agent.Configuration
                .RegisterComponent(new GrpcRemoteHostComponent(
                    new GrpcHostOptions
                    {
                        Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                        UseAsDefaultEngine = true
                    }));

            Console.ReadLine();

            var stateEngine = REstateHost.Agent
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
                    .AsSubstateOf("Ready")
                    .DescribedAs("An echo command failed to execute.")
                    .WithTransitionFrom("Ready", "EchoFailure"));

            var machine = stateEngine.CreateMachineAsync(schematic, null, CancellationToken.None).Result;

            var status = machine.SendAsync("Echo", "Hello from client!", CancellationToken.None).Result;

            Console.ReadLine();
        }
    }
}