using System;
using System.Collections.Generic;
using System.Linq;
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
        internal Dictionary<Type, State<TypeState, TypeState>> states = new Dictionary<Type, State<TypeState, TypeState>>();

        internal TypeState initialState;

        internal Schematic<TypeState, TypeState> schematic = new Schematic<TypeState, TypeState>();

        internal IAgent Agent { get; }

        public CreationContext(IAgent agent)
        {
            Agent = agent;
        }

        internal void StartsIn<TState>()
            where TState : IStateDefinition
        {
            TypeState typeState = typeof(TState);

            if (!typeState.AcceptsInputOf<TypeState>())
            {
                throw new ArgumentException($"State {typeState.AssemblyQualifiedName} does not accept valid input for this machine.");
            }

            var state = new State<TypeState, TypeState>
            {
                Value = typeState
            };

            if (typeState.IsActionable() || typeState.IsPrecondition())
            {
                Agent.Configuration.Register(registrar =>
                    registrar.RegisterConnector(typeState)
                        .WithConfiguration(new ConnectorConfiguration(typeState.ConnectorKey)));

                if (typeState.IsActionable())
                    state.Action = new REstate.Schematics.Action<TypeState>
                    {
                        ConnectorKey = typeState.ConnectorKey
                    };

                if (typeState.IsPrecondition())
                    state.Precondition = new Precondition
                    {
                        ConnectorKey = typeState.ConnectorKey
                    };
            }

            states.Add(typeState, state);

            schematic.InitialState = typeState;
        }

        public INaturalSchematic BuildAs(string schematicName)
        {
            schematic.SchematicName = schematicName;

            schematic.States = states.Values.ToArray();

            return new NaturalSchematic(schematic);
        }

        public IForStateContext<TState> For<TState>()
            where TState : IStateDefinition
        {
            TypeState typeState = typeof(TState);

            if (!states.TryGetValue(typeState, out var state))
            {
                if (!typeState.AcceptsInputOf<TypeState>())
                {
                    throw new ArgumentException($"State {typeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                state = new State<TypeState, TypeState>
                {
                    Value = typeof(TState)
                };

                if (typeState.IsActionable() || typeState.IsPrecondition())
                {
                    Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(typeState)
                            .WithConfiguration(new ConnectorConfiguration(typeState.ConnectorKey)));

                    if (typeState.IsActionable())
                        state.Action = new REstate.Schematics.Action<TypeState>
                        {
                            ConnectorKey = typeState.ConnectorKey
                        };

                    if (typeState.IsPrecondition())
                        state.Precondition = new Precondition
                        {
                            ConnectorKey = typeState.ConnectorKey
                        };
                }

                states.Add(typeState, state);
            }

            return new ForStateContext<TState>() { creationContext = this, state = state };
        }
    }

    internal class ForStateContext<TState>
        : IForStateContext<TState>
        where TState : IStateDefinition
    {
        internal CreationContext creationContext;

        internal State<TypeState, TypeState> state;

        public IOnContext<TState, TRequest> On<TRequest>()
        {
            return new OnContext<TState, TRequest> { creationContext = creationContext, state = state, input = typeof(TRequest) };
        }
    }

    internal class OnContext<TState, TSignal>
        : IOnContext<TState, TSignal>
        where TState : IStateDefinition
    {
        internal CreationContext creationContext;

        internal State<TypeState, TypeState> state;

        internal TypeState input;

        public ICreationContext MoveTo<TNewState>()
            where TNewState : IStateDefinition<TSignal>
        {
            TypeState newTypeState = typeof(TNewState);

            if (!creationContext.states.TryGetValue(newTypeState, out var newState))
            {
                if (!newTypeState.AcceptsInputOf<TypeState>())
                {
                    throw new ArgumentException($"State {newTypeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                newState = new State<TypeState, TypeState>
                {
                    Value = newTypeState
                };

                if (newTypeState.IsActionable() || newTypeState.IsPrecondition())
                {
                    creationContext.Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(newTypeState)
                            .WithConfiguration(new ConnectorConfiguration(newTypeState.ConnectorKey)));

                    if (newTypeState.IsActionable())
                        newState.Action = new REstate.Schematics.Action<TypeState>
                        {
                            ConnectorKey = newTypeState.ConnectorKey
                        };

                    if (newTypeState.IsPrecondition())
                        newState.Precondition = new Precondition
                        {
                            ConnectorKey = newTypeState.ConnectorKey
                        };
                }

                creationContext.states.Add(newTypeState, newState);
            }

            state.Transitions = (state.Transitions ?? new Transition<TypeState, TypeState>[0])
                .Concat(new[]
                {
                    new Transition<TypeState, TypeState>
                    {
                        Input = input,
                        ResultantState = newTypeState
                    }
                }).ToArray();

            return creationContext;
        }

        public IWhenContext<TState, TSignal> When<TPrecondition>()
        {
            TypeState preconditionTypeState = typeof(TPrecondition);

            if (!preconditionTypeState.IsPrecondition())
            {
                throw new ArgumentException($"Type {preconditionTypeState.AssemblyQualifiedName} does not implement {nameof(Engine.Connectors.IPrecondition)}.");
            }

            creationContext.Agent.Configuration.Register(registrar =>
                registrar.RegisterConnector(preconditionTypeState)
                    .WithConfiguration(new ConnectorConfiguration(preconditionTypeState.ConnectorKey)));

            return new WhenContext<TState, TSignal>
            {
                creationContext = creationContext,
                state = state,
                input = input,
                precondition = new Precondition
                {
                    ConnectorKey = preconditionTypeState.ConnectorKey
                }
            };
        }
    }

    internal class WhenContext<TState, TSignal>
        : IWhenContext<TState, TSignal>
        where TState : IStateDefinition
    {
        internal CreationContext creationContext;

        internal State<TypeState, TypeState> state;

        internal TypeState input;

        internal Precondition precondition;

        public ICreationContext MoveTo<TNewState>()
            where TNewState : IStateDefinition<TSignal>
        {
            TypeState newTypeState = typeof(TNewState);

            if (!creationContext.states.TryGetValue(newTypeState, out var newState))
            {
                if (!newTypeState.AcceptsInputOf<TypeState>())
                {
                    throw new ArgumentException($"State {newTypeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                newState = new State<TypeState, TypeState>
                {
                    Value = newTypeState
                };

                if (newTypeState.IsActionable() || newTypeState.IsPrecondition())
                {
                    creationContext.Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(newTypeState)
                            .WithConfiguration(new ConnectorConfiguration(newTypeState.ConnectorKey)));

                    if (newTypeState.IsActionable())
                        newState.Action = new REstate.Schematics.Action<TypeState>
                        {
                            ConnectorKey = newTypeState.ConnectorKey
                        };

                    if (newTypeState.IsPrecondition())
                        newState.Precondition = new Precondition
                        {
                            ConnectorKey = newTypeState.ConnectorKey
                        };
                }

                creationContext.states.Add(newTypeState, newState);
            }

            state.Transitions = (state.Transitions ?? new Transition<TypeState, TypeState>[0])
                .Concat(new[]
                {
                    new Transition<TypeState, TypeState>
                    {
                        Input = input,
                        ResultantState = newTypeState,
                        Precondition = precondition
                    }
                }).ToArray();

            return creationContext;
        }
    }
}
