using System;
using System.Collections.Generic;
using System.Linq;

namespace REstate.Schematics.TypedBuilder
{
    class TypedSchematicBuilder<TInput> : ITypedSchematicBuilder<TInput>
    {
        public ICreationContext<TInput> StartsIn<TState>()
        {
            var ctx = new CreationContext<TInput>();

            ctx.StartsIn<TState>();

            return ctx;
        }
    }

    class CreationContext<TInput> : ICreationContext<TInput>
    {
        internal Dictionary<Type, State<TypeState, TInput>> states = new Dictionary<Type, State<TypeState, TInput>>();

        internal TypeState initialState;

        internal Schematic<TypeState, TInput> schematic = new Schematic<TypeState, TInput>();

        internal void StartsIn<TState>()
        {
            TypeState typeState = typeof(TState);

            if (!typeState.AcceptsInputOf<TInput>())
            {
                throw new ArgumentException($"State {typeState.AssemblyQualifiedName} does not accept valid input for this machine.");
            }

            var state = new State<TypeState, TInput>
            {
                Value = typeof(TState)
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
        {
            if (!states.TryGetValue(typeof(TState), out var state))
            {
                TypeState typeState = typeof(TState);

                if (!typeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {typeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                state = new State<TypeState, TInput>
                {
                    Value = typeof(TState)
                };

                states.Add(typeof(TState), state);
            }

            return new ForStateContext<TState, TInput>() { creationContext = this, state = state };
        }
    }

    class ForStateContext<TState, TInput> : IForStateContext<TState, TInput>
    {
        internal CreationContext<TInput> creationContext;

        internal State<TypeState, TInput> state;

        public IOnContext<TState, TInput> On(TInput input)
        {
            return new OnContext<TState, TInput> { creationContext = creationContext, state = state, input = input };
        }
    }

    class OnContext<TState, TInput> : IOnContext<TState, TInput>
    {
        internal CreationContext<TInput> creationContext;

        internal State<TypeState, TInput> state;

        internal TInput input;

        public ICreationContext<TInput> MoveTo<TNewState>()
        {
            if (!creationContext.states.TryGetValue(typeof(TNewState), out var state))
            {
                TypeState newTypeState = typeof(TNewState);

                if (!newTypeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {newTypeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                state = new State<TypeState, TInput>
                {
                    Value = typeof(TNewState)
                };

                creationContext.states.Add(typeof(TNewState), state);
            }

            state.Transitions = state.Transitions.Concat(new[] 
            {
                new Transition<TypeState, TInput>
                {
                    Input = input,
                    ResultantState = typeof(TNewState)
                }
            }).ToArray();

            return creationContext;
        }

        public IWhenContext<TState, TInput> When<TPreconddition>()
        {
            TypeState preconditionTypeState = typeof(TPreconddition);

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
                    ConnectorKey = preconditionTypeState.AssemblyQualifiedName
                }
            };
        }
    }

    class WhenContext<TState, TInput> : IWhenContext<TState, TInput>
    {
        internal CreationContext<TInput> creationContext;

        internal State<TypeState, TInput> state;

        internal TInput input;

        internal Precondition precondition;

        public ICreationContext<TInput> MoveTo<TNewState>()
        {
            if (!creationContext.states.TryGetValue(typeof(TNewState), out var state))
            {
                TypeState newTypeState = typeof(TNewState);

                if (!newTypeState.AcceptsInputOf<TInput>())
                {
                    throw new ArgumentException($"State {newTypeState.AssemblyQualifiedName} does not accept valid input for this machine.");
                }

                state = new State<TypeState, TInput>
                {
                    Value = typeof(TNewState)
                };

                creationContext.states.Add(typeof(TNewState), state);
            }

            state.Transitions = state.Transitions.Concat(
                new[]
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
