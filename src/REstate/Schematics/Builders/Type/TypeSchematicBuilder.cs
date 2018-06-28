using System;
using System.Collections.Generic;
using System.Linq;

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
            where TState : IStateDefinition, new()
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
            where TState : IStateDefinition, new()
        {
            TypeState typeState = typeof(TState);

            if (!typeState.AcceptsInputOf<TInput>())
            {
                throw new ArgumentException($"State {typeState.AssemblyQualifiedName} does not accept valid input for this machine.");
            }

            Agent.Configuration.RegisterComponent<TState>();

            var state = new State<TypeState, TInput>
            {
                Value = typeof(TState),
                Action = new Action<TInput>
                {
                    ConnectorKey = typeof(TState).FullName
                },
                Precondition = new Precondition
                {
                    ConnectorKey = typeof(TState).FullName
                }
            };

            states.Add(typeof(TState), state);

            schematic.InitialState = typeof(TState);
        }

        public Schematic<TypeState, TInput> BuildAs(string schematicName)
        {
            schematic.SchematicName = schematicName;

            schematic.States = states.Values.ToArray();

            return schematic.Clone();
        }

        public IForStateContext<TState, TInput> For<TState>()
            where TState : IStateDefinition, new()
        {
            if (!states.TryGetValue(typeof(TState), out var state))
            {
                TypeState typeState = typeof(TState);

                if (!typeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {typeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                Agent.Configuration.RegisterComponent<TState>();

                state = new State<TypeState, TInput>
                {
                    Value = typeof(TState),
                    Action = new Action<TInput>
                    {
                        ConnectorKey = typeof(TState).FullName
                    },
                    Precondition = new Precondition
                    {
                        ConnectorKey = typeof(TState).FullName
                    }
                };

                states.Add(typeof(TState), state);
            }

            return new ForStateContext<TState, TInput>() { creationContext = this, state = state };
        }
    }

    internal class ForStateContext<TState, TInput>
        : IForStateContext<TState, TInput>
        where TState : IStateDefinition, new()
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
        where TState : IStateDefinition, new()
    {
        internal CreationContext<TInput> creationContext;

        internal State<TypeState, TInput> state;

        internal TInput input;

        public ICreationContext<TInput> MoveTo<TNewState>() 
            where TNewState : IStateDefinition, new()
        {
            if (!creationContext.states.TryGetValue(typeof(TNewState), out var newState))
            {
                TypeState newTypeState = typeof(TNewState);

                if (!newTypeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {newTypeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                creationContext.Agent.Configuration.RegisterComponent<TNewState>();

                newState = new State<TypeState, TInput>
                {
                    Value = typeof(TNewState),
                    Action = new Action<TInput>
                    {
                        ConnectorKey = typeof(TNewState).FullName
                    },
                    Precondition = new Precondition
                    {
                        ConnectorKey = typeof(TNewState).FullName
                    }
                };

                creationContext.states.Add(typeof(TNewState), newState);
            }

            state.Transitions = (state.Transitions ?? new Transition<TypeState, TInput>[0])
                .Concat(new[]
                {
                    new Transition<TypeState, TInput>
                    {
                        Input = input,
                        ResultantState = typeof(TNewState)
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
                    ConnectorKey = typeof(TPrecondition).FullName
                }
            };
        }
    }

    internal class WhenContext<TState, TInput>
        : IWhenContext<TState, TInput>
        where TState : IStateDefinition, new()
    {
        internal CreationContext<TInput> creationContext;

        internal State<TypeState, TInput> state;

        internal TInput input;

        internal Precondition precondition;

        public ICreationContext<TInput> MoveTo<TNewState>()
            where TNewState : IStateDefinition, new()
        {
            if (!creationContext.states.TryGetValue(typeof(TNewState), out var newState))
            {
                TypeState newTypeState = typeof(TNewState);

                if (!newTypeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {newTypeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                creationContext.Agent.Configuration.RegisterComponent<TNewState>();

                newState = new State<TypeState, TInput>
                {
                    Value = typeof(TNewState),
                    Action = new Action<TInput>
                    {
                        ConnectorKey = typeof(TNewState).FullName
                    },
                    Precondition = new Precondition
                    {
                        ConnectorKey = typeof(TNewState).FullName
                    }
                };

                creationContext.states.Add(typeof(TNewState), newState);
            }

            state.Transitions = (state.Transitions ?? new Transition<TypeState, TInput>[0])
                .Concat(new[]
                {
                    new Transition<TypeState, TInput>
                    {
                        Input = input,
                        ResultantState = typeof(TNewState),
                        Precondition = precondition
                    }
                }).ToArray();

            return creationContext;
        }
    }
}
