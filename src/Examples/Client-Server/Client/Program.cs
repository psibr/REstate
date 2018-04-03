using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using REstate;
using REstate.Remote;
using Serilog;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger = logger;

            REstateHost.Agent.Configuration
                .RegisterComponent(new GrpcRemoteHostComponent(
                    new GrpcHostOptions
                    {
                        Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                        UseAsDefaultEngine = true
                    }));

            var stateEngine = REstateHost.Agent
                .GetStateEngine<string, string>();

            var remoteLoggerSchematic = REstateHost.Agent
                .CreateSchematic<string, string>("LoggerMachine")

                .WithState("CreatedAndReady", state => state
                    .AsInitialState())

                .WithState("Ready", state => state
                    .WithTransitionFrom("CreatedAndReady", "log")
                    .WithReentrance("log")
                    .WithAction("log info", action => action
                        .DescribedAs("Logs the payload as a message.")
                        .WithSetting("messageFormat", "{schematicName}({machineId}) entered {state} on {input}. Message: {payload}")
                        .OnFailureSend("logFailure")))

                .WithState("LogFailure", state => state
                    .AsSubstateOf("Ready")
                    .DescribedAs("A message failed to log.")
                    .WithAction("log error", action => action
                        .DescribedAs("Logs the failure.")
                        .WithSetting("messageFormat", "Logging failed, message was: {payload}"))
                    .WithTransitionFrom("Ready", "logFailure"))

                .Build();

            // Metadata can be attached to each machine (an instance of a schematic). 
            // e.g. MachineName, or a correlationId of some kind.
            var metadata = new Dictionary<string, string>
            {
                ["MachineName"] = Environment.MachineName
            };

            try
            {
                var machine = await stateEngine.CreateMachineAsync(remoteLoggerSchematic, metadata);

                var result = await machine.SendAsync("log", "Hello from client!");

            }
            catch (Exception ex)
            {
                Log.Logger.Fatal("An error occured that crashed the app.", ex);
                throw;
            }

            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }
    }
}
