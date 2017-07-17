using System.ComponentModel.DataAnnotations;

namespace REstate.Schematics
{
    public class ExceptionTransition<TInput>
    {
        [Required]
        public TInput Input { get; set; }
    }
}
