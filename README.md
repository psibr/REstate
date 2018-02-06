[![Build status](https://ci.appveyor.com/api/projects/status/8ql1gahqjxcl8nlx/branch/master?svg=true)](https://ci.appveyor.com/project/psibr/restate-wuhpb/branch/master)
[![AppVeyor tests](https://img.shields.io/appveyor/tests/psibr/restate-wuhpb.svg?logo=appveyor)](https://ci.appveyor.com/project/psibr/restate-wuhpb/build/tests)

[![NuGet REstate Release](https://img.shields.io/nuget/v/REstate.svg?label=REstate)](https://www.nuget.org/packages/REstate)
[![NuGet REstate Concurrency Primitives Release](https://img.shields.io/nuget/v/REstate.Concurrency.Primitives.svg?label=REstate.Concurrency.Primitives)](https://www.nuget.org/packages/REstate.Concurrency.Primitives)
[![NuGet REstate Remote Release](https://img.shields.io/nuget/v/REstate.Remote.svg?label=REstate.Remote)](https://www.nuget.org/packages/REstate.Remote)

[![NuGet REstate Redis Repo Release](https://img.shields.io/nuget/v/REstate.Engine.Repositories.Redis.svg?label=REstate.Engine.Repositories.Redis)](https://www.nuget.org/packages/REstate.Engine.Repositories.Redis)
[![NuGet REstate EFCore Repo Release](https://img.shields.io/nuget/v/REstate.Engine.Repositories.Redis.svg?label=REstate.Engine.Repositories.EntityFrameworkCore)](https://www.nuget.org/packages/REstate.Engine.Repositories.EntityFrameworkCore)

![REstate Logo](https://github.com/psibr/REstate/blob/master/assets/icons/REstate.svg) 
> "Android, bionic, hand, machine, palm, robot, technology icon" by [designforeat](https://www.iconfinder.com/designforeat), used under [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/legalcode)/Partially rotated from original
# REstate

REstate is a portable state-flow system. It allows you to define a set of possible states that a program can exist in and what input will cause the transition between any two states. When it transitions into a given state, you can define an action that will execute arbitrary code. This can be used to build complex, but deterministic and predefined systems, such as workflow systems or distributed systems.

A `Schematic` may be defined on one server or workstation, but then used on others. `REstateMachine`s can call arbitrary code through the connector system, provided the connector exists on the system where it is running.

### Given the following REstate schematic, represented in DOT Graph:

![EchoMachine Schematic in DOT Graph](https://github.com/psibr/REstate/blob/master/LoggerMachine-Diagram.svg)

### Here is the code to build the schematic:

```csharp
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
```

### Schematics are serializer friendly.

The same schematic in YML (YamlDotNet):

```yml
SchematicName: LoggerMachine
InitialState: Created
States:
- Value: Created
  Transitions:
  - Input: log
    ResultantState: Ready
- Value: Ready
  Transitions:
  - Input: log
    ResultantState: Ready
  OnEntry:
    ConnectorKey:
      Identifier: log info
    Configuration:
      messageFormat: '{schematicName}({machineId}) entered {state} on {input}. Message: {payload}'
    Description: Logs the payload as a message.

```
As well as JSON or any other format really.
