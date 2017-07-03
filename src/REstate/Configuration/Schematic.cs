using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class Schematic<TState, TInput>
    {
        [Required]
        public string SchematicName { get; set; }

        [Required]
        public TState InitialState { get; set; }

        public StateConfiguration<TState, TInput>[] StateConfigurations { get; set; }
    }
}
