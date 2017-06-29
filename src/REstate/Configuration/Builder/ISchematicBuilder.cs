using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public interface ISchematicBuilder
        : ISchematic
    { 
        ISchematicBuilder WithState(string stateName, Action<IStateBuilder> state = null);
        ISchematicBuilder WithStates(ICollection<string> stateNames, Action<IStateBuilder> state = null);
        ISchematicBuilder WithStates(params string[] stateNames);

        ISchematicBuilder WithTransition(string stateName, Input input, string resultantStateName, Action<ITransitionBuilder> transition = null);
    }

    public interface ISchematic
    {
        string SchematicName { get; }
        string InitialState { get; }
        IReadOnlyDictionary<string, IState> States { get; }

        Schematic ToSchematic();
    }
}