using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class Schematic
    {
        [Required]
        public string SchematicName { get; set; }

        [Required]
        public string InitialState { get; set; }

        public StateConfiguration[] StateConfigurations { get; set; }
        
        public ServiceState[] ServiceStates { get; set; }
    }
}
