# REstate

REstate is a library that allows you to define portable state-flows (state-machine based workflows).

A `Schematic` may be defined on one server or workstation, but then used on others. 

`REstateMachine`s can call arbitrary code through the connector system, provided the connector exists on the system where it is running.

Given the following REstate schematic, represented in DOT Graph:

![EchoMachine Schematic in DOT Graph](https://github.com/psibr/REstate.Engine/blob/master/LoggerMachine-Diagram.svg)

Here is the code to build the schematic:

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

Schematics can be serialized into or from YML (YamlDotNet):

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
