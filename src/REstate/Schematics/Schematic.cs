using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace REstate.Schematics
{
    public class Schematic<TState, TInput> 
        : ISchematic<TState, TInput>
    {
        [Required]
        public string SchematicName { get; set; }

        [Required]
        public TState InitialState { get; set; }

        IReadOnlyDictionary<TState, IState<TState, TInput>> ISchematic<TState, TInput>.States => 
            StateConfigurations.ToDictionary(
                c => c.Value,
                c => (IState<TState, TInput>)c);

        public StateConfiguration<TState, TInput>[] StateConfigurations { get; set; }
    }
}
