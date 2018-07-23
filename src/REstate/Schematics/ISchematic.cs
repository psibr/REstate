using System.Collections.Generic;

namespace REstate.Schematics
{
    public interface ISchematic<TState, TInput>
    {
        string SchematicName { get; }
        TState InitialState { get; }
        int StateConflictRetryCount { get; }
        IReadOnlyDictionary<TState, IState<TState, TInput>> States { get; }
    }

    public interface ITypeSchematic : ISchematic<TypeState, TypeState>
    {

    }
}