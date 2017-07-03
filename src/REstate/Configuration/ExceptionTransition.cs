using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class ExceptionTransition<TInput>
    {
        [Required]
        public TInput Input { get; set; }
    }
}
