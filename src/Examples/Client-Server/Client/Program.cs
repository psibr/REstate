using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using REstate;
using REstate.Remote;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);

            REstateHost.Agent.Configuration
                .RegisterComponent(new GrpcRemoteHostComponent(
                    new GrpcHostOptions
                    {
                        Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                        UseAsDefaultEngine = true
                    }));

            var stateEngine = REstateHost.Agent
                .GetStateEngine<string, string>();

            var schematic = REstateHost.Agent
                .CreateSchematic<string, string>("EchoMachine")

                .WithState("Ready", state => state
                    .WithOnEntry(new ConnectorKey("Log"), onEntry => onEntry
                        .DescribedAs("Echoes the payload to the console.")
                        .WithSetting("message", "{3}")
                        .OnFailureSend("EchoFailure"))
                    .WithReentrance("Echo"))

                .WithState("CreatedAndReady", state => state
                    .AsInitialState()
                    .AsSubstateOf("Ready")
                    .WithTransitionTo("Ready", "Echo"))

                .WithState("EchoFailure", state => state
                    .AsSubstateOf("Ready")
                    .DescribedAs("An echo command failed to execute.")
                    .WithTransitionFrom("Ready", "EchoFailure"));

            var machines = Enumerable.Range(0, 4).Select(i =>
                stateEngine.CreateMachineAsync(schematic, null, CancellationToken.None).GetAwaiter().GetResult());

            Parallel.ForEach(machines, machine =>
                {
                    for (int i = 0; i < 999; i++)
                    {
                        machine.SendAsync("Echo", "Hello from client!", CancellationToken.None).GetAwaiter().GetResult();
                    }
                    
                });


            Console.ReadLine();
        }
    }
}