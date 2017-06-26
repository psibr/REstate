using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class ExceptionTransition
    {
        [Required]
        public string TriggerName { get; set; }
    }
}
