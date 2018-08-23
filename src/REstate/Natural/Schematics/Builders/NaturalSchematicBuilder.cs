using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using REstate.Engine.Connectors;
using REstate.IoC;
using REstate.Schematics;

namespace REstate.Natural.Schematics.Builders
{
    internal class NaturalSchematicBuilder
        : INaturalSchematicBuilder
    {
        public NaturalSchematicBuilder(IAgent agent)
        {
            Agent = agent;
        }

        internal IAgent Agent { get; }

        public ICreationContext StartsIn<TState>()
            where TState : IStateDefinition
        {
            var ctx = new CreationContext(Agent);

            ctx.StartsIn<TState>();

            return ctx;
        }
    }

    internal class CreationContext
        : ICreationContext
    {
        internal Dictionary<Type, State<TypeState, TypeState>> States = new Dictionary<Type, State<TypeState, TypeState>>();

        internal TypeState InitialState;

        internal Schematic<TypeState, TypeState> Schematic = new Schematic<TypeState, TypeState>();

        internal IAgent Agent { get; }

        public CreationContext(IAgent agent)
        {
            Agent = agent;
        }

        internal void StartsIn<TState>()
            where TState : IStateDefinition
        {
            TypeState typeState = typeof(TState);

            var state = new State<TypeState, TypeState>
            {
                Value = typeState
            };

            var description = typeof(TState).GetCustomAttribute<DescriptionAttribute>(false)?.Description;

            state.Description = description;

            if (typeState.IsActionable() || typeState.IsPrecondition())
            {
                Agent.Configuration.Register(registrar =>
                    registrar.RegisterConnector(typeState)
                        .WithConfiguration(new ConnectorConfiguration(typeState.GetConnectorKey())));

                if (typeState.IsActionable())
                    state.Action = new REstate.Schematics.Action<TypeState>
                    {
                        ConnectorKey = typeState.GetConnectorKey()
                    };

                if (typeState.IsPrecondition())
                    state.Precondition = new Precondition
                    {
                        ConnectorKey = typeState.GetConnectorKey()
                    };
            }

            States.Add(typeState, state);

            Schematic.InitialState = typeState;
        }

        public INaturalSchematic BuildAs(string schematicName)
        {
            Schematic.SchematicName = schematicName;

            Schematic.States = States.Values.ToArray();

            return new NaturalSchematic(Schematic);
        }

        public IForStateContext<TState> For<TState>()
            where TState : IStateDefinition
        {
            TypeState typeState = typeof(TState);

            if (!States.TryGetValue(typeState, out var state))
            {
                state = new State<TypeState, TypeState>
                {
                    Value = typeof(TState)
                };

                var description = typeof(TState).GetCustomAttribute<DescriptionAttribute>(false)?.Description;

                state.Description = description;

                if (typeState.IsActionable() || typeState.IsPrecondition())
                {
                    Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(typeState)
                            .WithConfiguration(new ConnectorConfiguration(typeState.GetConnectorKey())));

                    if (typeState.IsActionable())
                        state.Action = new REstate.Schematics.Action<TypeState>
                        {
                            ConnectorKey = typeState.GetConnectorKey()
                        };

                    if (typeState.IsPrecondition())
                        state.Precondition = new Precondition
                        {
                            ConnectorKey = typeState.GetConnectorKey(),
                        };
                }

                States.Add(typeState, state);
            }

            return new ForStateContext<TState>() { CreationContext = this, State = state };
        }
    }

    internal class ForStateContext<TState>
        : IForStateContext<TState>
        where TState : IStateDefinition
    {
        internal CreationContext CreationContext;

        internal State<TypeState, TypeState> State;

        public IOnContext<TState, TSignal> On<TSignal>()
        {
            return new OnContext<TState, TSignal>
            {
                CreationContext = CreationContext,
                State = State,
                Input = typeof(TSignal)
            };
        }
    }

    internal class OnContext<TState, TSignal>
        : IOnContext<TState, TSignal>
        where TState : IStateDefinition
    {
        internal CreationContext CreationContext;

        internal State<TypeState, TypeState> State;

        internal TypeState Input;

        public ICreationContext MoveTo<TNewState>()
            where TNewState : IStateDefinition<TSignal>
        {
            TypeState newTypeState = typeof(TNewState);

            if (!CreationContext.States.TryGetValue(newTypeState, out var newState))
            { 
                newState = new State<TypeState, TypeState>
                {
                    Value = newTypeState
                };

                var description = typeof(TNewState).GetCustomAttribute<DescriptionAttribute>(false)?.Description;

                newState.Description = description;

                if (newTypeState.IsActionable() || newTypeState.IsPrecondition())
                {
                    CreationContext.Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(newTypeState)
                            .WithConfiguration(new ConnectorConfiguration(newTypeState.GetConnectorKey())));

                    if (newTypeState.IsActionable())
                        newState.Action = new REstate.Schematics.Action<TypeState>
                        {
                            ConnectorKey = newTypeState.GetConnectorKey()
                        };

                    if (newTypeState.IsPrecondition())
                        newState.Precondition = new Precondition
                        {
                            ConnectorKey = newTypeState.GetConnectorKey()
                        };
                }

                CreationContext.States.Add(newTypeState, newState);
            }

            State.Transitions = (State.Transitions ?? new Transition<TypeState, TypeState>[0])
                .Concat(new[]
                {
                    new Transition<TypeState, TypeState>
                    {
                        Input = Input,
                        ResultantState = newTypeState
                    }
                }).ToArray();

            return CreationContext;
        }

        public IWhenContext<TState, TSignal> When<TPrecondition>()
            where TPrecondition : INaturalPrecondition<TSignal>
        {
            TypeState preconditionTypeState = typeof(TPrecondition);

            CreationContext.Agent.Configuration.Register(registrar =>
                registrar.RegisterConnector(preconditionTypeState)
                    .WithConfiguration(new ConnectorConfiguration(preconditionTypeState.GetConnectorKey())));

            return new WhenContext<TState, TSignal>
            {
                CreationContext = CreationContext,
                State = State,
                Input = Input,
                Precondition = new Precondition
                {
                    ConnectorKey = preconditionTypeState.GetConnectorKey()
                }
            };
        }
    }

    internal class WhenContext<TState, TSignal>
        : IWhenContext<TState, TSignal>
        where TState : IStateDefinition
    {
        internal CreationContext CreationContext;

        internal State<TypeState, TypeState> State;

        internal TypeState Input;

        internal Precondition Precondition;

        public ICreationContext MoveTo<TNewState>()
            where TNewState : IStateDefinition<TSignal>
        {
            TypeState newTypeState = typeof(TNewState);

            if (!CreationContext.States.TryGetValue(newTypeState, out var newState))
            {
                newState = new State<TypeState, TypeState>
                {
                    Value = newTypeState
                };

                var description = typeof(TNewState).GetCustomAttribute<DescriptionAttribute>(false)?.Description;

                newState.Description = description;

                if (newTypeState.IsActionable() || newTypeState.IsPrecondition())
                {
                    CreationContext.Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(newTypeState)
                            .WithConfiguration(new ConnectorConfiguration(newTypeState.GetConnectorKey())));

                    if (newTypeState.IsActionable())
                        newState.Action = new REstate.Schematics.Action<TypeState>
                        {
                            ConnectorKey = newTypeState.GetConnectorKey()
                        };

                    if (newTypeState.IsPrecondition())
                        newState.Precondition = new Precondition
                        {
                            ConnectorKey = newTypeState.GetConnectorKey()
                        };
                }

                CreationContext.States.Add(newTypeState, newState);
            }

            State.Transitions = (State.Transitions ?? new Transition<TypeState, TypeState>[0])
                .Concat(new[]
                {
                    new Transition<TypeState, TypeState>
                    {
                        Input = Input,
                        ResultantState = newTypeState,
                        Precondition = Precondition
                    }
                }).ToArray();

            return CreationContext;
        }
    }
}
