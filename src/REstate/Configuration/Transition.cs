using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class Transition
    {
        [Required]
        public string InputName { get; set; }

        [Required]
        public string ResultantStateName { get; set; }

        public GuardConnector Guard { get; set; }
    }
}
