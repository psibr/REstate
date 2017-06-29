using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class ExceptionTransition
    {
        [Required]
        public string Input { get; set; }
    }
}
