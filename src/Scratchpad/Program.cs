using System;
using System.Collections.Generic;
using System.Threading;
using REstate;
using Grpc.Core;
using System.Threading.Tasks;
using Grpc.Core.Logging;
using REstate.Engine;
using REstate.Remote;
using REstate.Schematics;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using YamlDotNet.Serialization;

namespace Scratchpad
{
    class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            var stateEngine = REstateHost.Agent
                .GetStateEngine<string, string>();

var schematic = REstateHost.Agent
    .CreateSchematic<string, string>("LoggerMachine")

    .WithState("Created", state => state
        .AsInitialState())

    .WithState("Ready", state => state
        .WithTransitionFrom("Created", "log")
        .WithReentrance("log")
        .WithOnEntry("log info", onEntry => onEntry
            .DescribedAs("Logs the payload as a message.")
            .WithSetting(
                key: "messageFormat", 
                value: "{schematicName}({machineId}) entered {state} on {input}. Message: {payload}")))

    .Build();

            //new YamlDotNet.Serialization.DeserializerBuilder().Build().Deserialize<Schematic<string, string>>();

            var yaml = new Serializer().Serialize(schematic);

            var diagram = schematic.WriteStateMap();
        }
    }
}