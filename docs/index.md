## Synopsis

REstate is a _portable state-flow_ system. It allows you to define a set of possible states that a program can exist in and what _input_ will cause the transition between any two states. When it transitions into a given state, you can define an action that will execute arbitrary code. This can be used to build complex, but deterministic and predefined systems, such as workflow systems or distributed systems.

## Portablity
REstate maintains portability by keeping the definitions serializable and referencing _connectors_, which provide the actions for the state, by name rather than a direct C# reference to a Lambda or similar construct. This means a REstate Machine can execute anywhere, as long as all systems that intend to execute the code have the connectors as well. This can be done by all agents using the same repository, or by utilizing the REstate.Remote plugin.
