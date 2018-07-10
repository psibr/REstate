using System;
using System.Collections.Generic;
using System.Linq;
using REstate.Engine.Connectors;
using REstate.IoC;

namespace REstate.Schematics.Builders
{
    internal class TypeSchematicBuilder<TInput>
        : ITypeSchematicBuilder<TInput>
    {
        public TypeSchematicBuilder(IAgent agent)
        {
            Agent = agent;
        }

        internal IAgent Agent { get; }

        public ICreationContext<TInput> StartsIn<TState>()
            where TState : IStateDefinition
        {
            var ctx = new CreationContext<TInput>(Agent);

            ctx.StartsIn<TState>();

            return ctx;
        }
    }

    internal class CreationContext<TInput>
        : ICreationContext<TInput>
    {
        internal Dictionary<Type, State<TypeState, TInput>> states = new Dictionary<Type, State<TypeState, TInput>>();

        internal TypeState initialState;

        internal Schematic<TypeState, TInput> schematic = new Schematic<TypeState, TInput>();

        internal IAgent Agent { get; }

        public CreationContext(IAgent agent)
        {
            Agent = agent;
        }

        internal void StartsIn<TState>()
            where TState : IStateDefinition
        {
            TypeState typeState = typeof(TState);

            if (!typeState.AcceptsInputOf<TInput>())
            {
                throw new ArgumentException($"State {typeState.AssemblyQualifiedName} does not accept valid input for this machine.");
            }

            var state = new State<TypeState, TInput>
            {
                Value = typeState
            };

            if (typeState.IsActionable() || typeState.IsPrecondition())
            {
                Agent.Configuration.Register(registrar =>
                    registrar.RegisterConnector(typeState)
                        .WithConfiguration(new ConnectorConfiguration(typeState.ConnectorKey)));

                if (typeState.IsActionable())
                    state.Action = new Action<TInput>
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

        public Schematic<TypeState, TInput> BuildAs(string schematicName)
        {
            schematic.SchematicName = schematicName;

            schematic.States = states.Values.ToArray();

            return schematic.Clone();
        }

        public IForStateContext<TState, TInput> For<TState>()
            where TState : IStateDefinition
        {
            TypeState typeState = typeof(TState);

            if (!states.TryGetValue(typeState, out var state))
            {
                if (!typeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {typeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                state = new State<TypeState, TInput>
                {
                    Value = typeof(TState)
                };

                if (typeState.IsActionable() || typeState.IsPrecondition())
                {
                    Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(typeState)
                            .WithConfiguration(new ConnectorConfiguration(typeState.ConnectorKey)));

                    if (typeState.IsActionable())
                        state.Action = new Action<TInput>
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

            return new ForStateContext<TState, TInput>() { creationContext = this, state = state };
        }
    }

    internal class ForStateContext<TState, TInput>
        : IForStateContext<TState, TInput>
        where TState : IStateDefinition
    {
        internal CreationContext<TInput> creationContext;

        internal State<TypeState, TInput> state;

        public IOnContext<TState, TInput> On(TInput input)
        {
            return new OnContext<TState, TInput> { creationContext = creationContext, state = state, input = input };
        }
    }

    internal class OnContext<TState, TInput>
        : IOnContext<TState, TInput>
        where TState : IStateDefinition
    {
        internal CreationContext<TInput> creationContext;

        internal State<TypeState, TInput> state;

        internal TInput input;

        public ICreationContext<TInput> MoveTo<TNewState>()
            where TNewState : IStateDefinition
        {
            TypeState newTypeState = typeof(TNewState);

            if (!creationContext.states.TryGetValue(newTypeState, out var newState))
            {
                if (!newTypeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {newTypeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                newState = new State<TypeState, TInput>
                {
                    Value = newTypeState
                };

                if (newTypeState.IsActionable() || newTypeState.IsPrecondition())
                {
                    creationContext.Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(newTypeState)
                            .WithConfiguration(new ConnectorConfiguration(newTypeState.ConnectorKey)));

                    if (newTypeState.IsActionable())
                        newState.Action = new Action<TInput>
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

            state.Transitions = (state.Transitions ?? new Transition<TypeState, TInput>[0])
                .Concat(new[]
                {
                    new Transition<TypeState, TInput>
                    {
                        Input = input,
                        ResultantState = newTypeState
                    }
                }).ToArray();

            return creationContext;
        }

        public IWhenContext<TState, TInput> When<TPrecondition>()
        {
            TypeState preconditionTypeState = typeof(TPrecondition);

            if (!preconditionTypeState.IsPrecondition())
            {
                throw new ArgumentException($"Type {preconditionTypeState.AssemblyQualifiedName} does not implement {nameof(Engine.Connectors.IPrecondition)}.");
            }

            return new WhenContext<TState, TInput>
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

    internal class WhenContext<TState, TInput>
        : IWhenContext<TState, TInput>
        where TState : IStateDefinition
    {
        internal CreationContext<TInput> creationContext;

        internal State<TypeState, TInput> state;

        internal TInput input;

        internal Precondition precondition;

        public ICreationContext<TInput> MoveTo<TNewState>()
            where TNewState : IStateDefinition
        {
            TypeState newTypeState = typeof(TNewState);

            if (!creationContext.states.TryGetValue(newTypeState, out var newState))
            {
                if (!newTypeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {newTypeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                newState = new State<TypeState, TInput>
                {
                    Value = newTypeState
                };

                if (newTypeState.IsActionable() || newTypeState.IsPrecondition())
                {
                    creationContext.Agent.Configuration.Register(registrar =>
                        registrar.RegisterConnector(newTypeState)
                            .WithConfiguration(new ConnectorConfiguration(newTypeState.ConnectorKey)));

                    if (newTypeState.IsActionable())
                        newState.Action = new Action<TInput>
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

            state.Transitions = (state.Transitions ?? new Transition<TypeState, TInput>[0])
                .Concat(new[]
                {
                    new Transition<TypeState, TInput>
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
