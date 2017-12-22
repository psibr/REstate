using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using REstate.Engine;

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

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => 
            DotGraphCartographer<TState, TInput>.Instance.WriteMap(States);
    }
}
