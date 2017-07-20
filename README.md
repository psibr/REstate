# REstate.Engine

Given the following REstate schematic, represented in DOT Graph:

![Alt EchoMachine Schematic in DOT Graph](https://cdn.rawgit.com/psibr/REstate.Engine/92d7bbe2/echo-sample.svg)

Here is the code to build the flow and echo "Hello!":

```csharp
using System;
using System.Threading;
using REstate;

namespace Scratchpad
{ 
    class Program
    {
        static void Main(string[] args)
        {
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
```

It can also be created in YML or JSON, or any format:

```yml
SchematicName: EchoMachine
InitialState: Ready
States:
- Value: Ready
  Transitions:
  - Input: Echo
    ResultantState: Ready
    Guard:
      ConnectorKey: Console
      Configuration:
        Prompt: Are you sure you want to echo "{3}"? (y/n)
  - Input: EchoFailure
    ResultantState: EchoFailure
  OnEntry:
    ConnectorKey: Console
    Configuration:
      Format: '{2}'
    Description: Echoes the payload to the console.
    FailureTransition:
      Input: EchoFailure
- Value: EchoFailure
  ParentState: Ready
  Description: An echo command failed to execute.
  Transitions: []
```
