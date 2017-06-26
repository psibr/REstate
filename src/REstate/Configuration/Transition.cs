using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class Transition
    {
        [Required]
        public string TriggerName { get; set; }

        [Required]
        public string ResultantStateName { get; set; }

        public GuardConnector Guard { get; set; }
    }
}
