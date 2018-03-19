using REstate.Schematics.Builder.Providers;
using System;
using System.Collections.Generic;

namespace REstate.Schematics.Builder.Implementation.Typed
{
    public interface ITypedStateSchematicBuilder<TInput>
        : ISchematicBuilderProvider<Type, TInput, ITypedStateSchematicBuilder<TInput>>
    {
        ITypedStateSchematicBuilder<TInput> WithState<T>(System.Action<IStateBuilder<Type, TInput>> stateBuilderAction = null);

        ITypedStateSchematicBuilder<TInput> WithTransition<TSourceState, TResultantState>(TInput input, System.Action<ITransitionBuilder<Type, TInput>> transitionBuilderAction = null);
    }

    public class TypedStateSchematicBuilder<TInput>
        : ITypedStateSchematicBuilder<TInput>
    {
        private SchematicBuilder<Type, TInput> Builder { get; }

        public TypedStateSchematicBuilder(string schematicName)
        {
            Builder = new SchematicBuilder<Type, TInput>(schematicName);
        }

        public TypedStateSchematicBuilder(SchematicBuilder<Type, TInput> builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public string SchematicName => Builder.SchematicName;

        public Type InitialState => Builder.InitialState;

        public int StateConflictRetryCount => Builder.StateConflictRetryCount;

        public IReadOnlyDictionary<Type, IState<Type, TInput>> States => Builder.States;

        public Schematic<Type, TInput> Build()
        {
            return Builder.Build();
        }

        public ITypedStateSchematicBuilder<TInput> WithState<T>(System.Action<IStateBuilder<Type, TInput>> stateBuilderAction = null)
        {
            WithState(typeof(T), stateBuilderAction);

            return this;
        }

        public ITypedStateSchematicBuilder<TInput> WithState(Type state, System.Action<IStateBuilder<Type, TInput>> stateBuilderAction = null)
        {
            Builder.WithState(state, stateBuilderAction);

            return this;
        }

        public ITypedStateSchematicBuilder<TInput> WithStateConflictRetries()
        {
            Builder.WithStateConflictRetries();

            return this;
        }

        public ITypedStateSchematicBuilder<TInput> WithStateConflictRetries(int retryCount)
        {
            Builder.WithStateConflictRetries(retryCount);

            return this;
        }

        public ITypedStateSchematicBuilder<TInput> WithStates(ICollection<Type> states, System.Action<IStateBuilder<Type, TInput>> stateBuilderAction = null)
        {
            Builder.WithStates(states, stateBuilderAction);

            return this;
        }

        public ITypedStateSchematicBuilder<TInput> WithStates(params Type[] states)
        {
            Builder.WithStates(states);

            return this;
        }

        public ITypedStateSchematicBuilder<TInput> WithTransition(Type sourceState, TInput input, Type resultantState, System.Action<ITransitionBuilder<Type, TInput>> transitionBuilderAction = null)
        {
            Builder.WithTransition(sourceState, input, resultantState, transitionBuilderAction);

            return this;
        }

        public ITypedStateSchematicBuilder<TInput> WithTransition<TSourceState, TResultantState>(TInput input, System.Action<ITransitionBuilder<Type, TInput>> transitionBuilderAction = null)
        {
            Builder.WithTransition(typeof(TSourceState), input, typeof(TResultantState), transitionBuilderAction);

            return this;
        }
    }
}
