using System;
using System.Collections.Generic;

namespace REstate.Configuration.Builder
{
    public interface ISchematicBuilder
    { 
        string SchematicName { get; }
        string InitialState { get; }

        IReadOnlyDictionary<string, IStateBuilder> States { get; }

        ISchematicBuilder WithState(string stateName, Action<IStateBuilder> stateBuilder = null);
        ISchematicBuilder WithStates(ICollection<string> stateNames, Action<IStateBuilder> stateBuilder = null);
        ISchematicBuilder WithStates(params string[] stateNames);

        ISchematicBuilder WithTransition(string stateName, Input input, string resultantStateName, GuardConnector guard = null);

        Schematic ToSchematic();
    }
}