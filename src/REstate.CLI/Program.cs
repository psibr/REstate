using System;
using System.Threading;
using Grpc.Core;
using REstate.CLI.CommandLine;
using REstate.Remote;

namespace REstate.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var command = Commands.None;
            var schematicName = string.Empty;
            var remoteEndoint = "0.0.0.0:12345";
            var stateType = typeof(string);
            var inputType = typeof(string);

            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineCommand("schematics", ref command, Commands.Schematics, "Opertions that can be performed on schematics.");
                syntax.DefineParameter("name", ref schematicName, "The SchematicName.");
                syntax.DefineOption("e|endpoint", ref remoteEndoint, "Endpoint of remote server. Defaults to 0.0.0.0:12345");
                syntax.DefineOption("s|stateType", ref stateType, Type.GetType, false, "The CLR type name of the state.");
                syntax.DefineOption("i|inputType", ref inputType, Type.GetType, false, "The CLR type name of the input.");

                if(command == Commands.Schematics)
                    if(schematicName == string.Empty)
                        syntax.ReportError("name is a required parameter for schematics");
            });

            REstateHost.Agent.Configuration.RegisterComponent(new GrpcRemoteHostComponent(new GrpcHostOptions
            {
                Channel = new Channel(remoteEndoint, ChannelCredentials.Insecure),
                UseAsDefaultEngine = true
            }));


            var yamlSerializer = new YamlDotNet.Serialization.Serializer();

            if (command == Commands.Schematics)
            {
                Console.WriteLine(yamlSerializer.Serialize(
                    REstateHost.Agent.GetStateEngine<string, string>()
                        .GetSchematicAsync(schematicName, CancellationToken.None)));
            }
            
        }
    }

    public enum Commands
    {
        None,
        Schematics,
        Machines
    }
}
