using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace REstate.Schematics
{
    public class Schematic<TState, TInput> 
        : ISchematic<TState, TInput>
    {
        public Schematic(string schematicName, TState initialState, State<TState, TInput>[] states)
        {
            SchematicName = schematicName;
            InitialState = initialState;
            States = states;
        }

        public Schematic()
        {
        }

        [Required]
        public string SchematicName { get; set; }

        [Required]
        public TState InitialState { get; set; }

        IReadOnlyDictionary<TState, IState<TState, TInput>> ISchematic<TState, TInput>.States => 
            States.ToDictionary(
                c => c.Value,
                c => (IState<TState, TInput>)c);

        public State<TState, TInput>[] States { get; set; }
    }
}
