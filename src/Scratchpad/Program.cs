using System;
using System.IO;
using System.Threading;
using Avro;
using REstate;
using REstate.Configuration;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using System;
using System.Threading.Tasks;
using MagicOnion.Client;
using MessagePack;

namespace Scratchpad
{
    // define interface as Server/Client IDL.
    // implements T : IService<T>.
    public interface IMyFirstService : IService<IMyFirstService>
    {
        UnaryResult<T> TestAsync<T>(Test<T> t);
    }

    // implement RPC service.
    // inehrit ServiceBase<interface>, interface
    public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
    {
        public async UnaryResult<T> TestAsync<T>(Test<T> t)
        {
            Logger.Debug($"Received:{t.Property}");

            return t.Property;
        }
    }

    [MessagePackObject]
    public class Test<T>
    {
        [Key(0)]
        public T Property { get; set; }
    }

    class Program
    {

        static async void ClientImpl()
        {
            // standard gRPC channel
            var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);

            // create MagicOnion dynamic client proxy
            var client = MagicOnionClient.Create<IMyFirstService>(channel);

            // call method.
            var result = await client.TestAsync(new Test<string>{ Property = "Dick"});
            Console.WriteLine("Client Received:" + result);
        }

        static void Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

            var service = MagicOnionEngine.BuildServerServiceDefinition(isReturnExceptionStackTraceInErrorDetail: true);

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


            var schematic = REstateHost
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



            var echoMachine = REstateHost.GetStateEngine<string, string>().CreateMachineAsync(schematic, null, CancellationToken.None).Result;

            var graph = echoMachine.ToString();

            var status = echoMachine.SendAsync("Echo", "Hello!", CancellationToken.None).Result;

            Console.ReadLine();
        }
    }
}