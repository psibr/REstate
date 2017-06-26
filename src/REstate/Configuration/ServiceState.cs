using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class ServiceState
    {
        [Required]
        public string StateName { get; set; }

        public string ParentStateName { get; set; }

        public string Description { get; set; }

        [Required]
        public bool UseAcceptAndRejectStates { get; set; }

        public Transition[] Transitions { get; set; }

        public long? RetryDelaySeconds { get; set; }

        public ServiceEntryConnector OnEntry { get; set; }
    }
}