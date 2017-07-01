# REstate.Engine
The core and engine of REstate.

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
                .CreateSchematic("EchoMachine")

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

            var echoMachine = REstateHost.Engine.CreateMachineAsync(
                schematic: schematic,
                metadata: null,
                cancellationToken: CancellationToken.None).Result;

            var status = echoMachine.SendAsync(
                input: "Echo",
                payload: "Hello!",
                cancellationToken: CancellationToken.None).Result;
        }
    }
}
```
