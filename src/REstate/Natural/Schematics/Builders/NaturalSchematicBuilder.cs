using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using REstate.Schematics;

namespace REstate.Natural.Schematics.Builders
{
    internal class NaturalSchematicBuilder
        : INaturalSchematicBuilder
    {
        public NaturalSchematicBuilder()
        {
        }

        public static State<TypeState, TypeState> CreateState<TState>()
            where TState : IStateDefinition
        {
            var type = typeof(TState);
            TypeState typeState = type;

            var state = new State<TypeState, TypeState>
            {
                Value = typeState
            };

            var description = typeof(TState).GetCustomAttribute<DescriptionAttribute>(false)?.Description;

            state.Description = description;

            if (typeState.IsActionable() || typeState.IsPrecondition())
            {
                if (typeState.IsActionable())
                {
                    string actionDescription = null;
                    var longRunning = false;

                    var naturalActionInterface = type.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType
                                             && i.GetGenericTypeDefinition() ==
                                             typeof(INaturalAction<>));

                    if (naturalActionInterface != null)
                    {
                        var invokeMethodInfo = type.GetMethod(nameof(INaturalAction<object>.InvokeAsync),
                            new[]
                            {
                                typeof(ConnectorContext),
                                naturalActionInterface.GenericTypeArguments[0],
                                typeof(CancellationToken)
                            });

                        actionDescription = invokeMethodInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description;
                        longRunning = !(invokeMethodInfo?.GetCustomAttribute<LongRunningAttribute>() is null);
                    }

                    state.Action = new REstate.Schematics.Action<TypeState>
                    {
                        ConnectorKey = typeState.GetConnectorKey(),
                        Description = actionDescription,
                        LongRunning = longRunning
                    };
                }

                if (typeState.IsPrecondition())
                    state.Precondition = new Precondition
                    {
                        ConnectorKey = typeState.GetConnectorKey()
                    };
            }

            return state;
        }

        public ICreationContext StartsIn<TState>()
            where TState : IStateDefinition
        {
            var ctx = new CreationContext();

            ctx.StartsIn<TState>();

            return ctx;
        }

        internal class CreationContext
            : ICreationContext
        {
            internal readonly Dictionary<Type, State<TypeState, TypeState>> States =
                new Dictionary<Type, State<TypeState, TypeState>>();

            private readonly Schematic<TypeState, TypeState> _schematic = new Schematic<TypeState, TypeState>();
           
            public CreationContext()
            {
            }

            internal void StartsIn<TState>()
                where TState : IStateDefinition
            {
                TypeState typeState = typeof(TState);

                var state = CreateState<TState>();

                States.Add(typeState, state);

                _schematic.InitialState = typeState;
            }

            public INaturalSchematic BuildAs(string schematicName)
            {
                _schematic.SchematicName = schematicName;

                _schematic.States = States.Values.ToArray();

                return new NaturalSchematic(_schematic);
            }

            public IForStateContext<TState> For<TState>()
                where TState : IStateDefinition
            {
                TypeState typeState = typeof(TState);

                if (!States.TryGetValue(typeState, out var state))
                {
                    state = CreateState<TState>();

                    States.Add(typeState, state);
                }

                return new ForStateContext<TState> { CreationContext = this, State = state };
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
                    newState = CreateState<TNewState>();

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
                    newState = CreateState<TNewState>();

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


}