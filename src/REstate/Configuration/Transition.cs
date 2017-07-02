using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class Transition<TState>
    {
        [Required]
        public string InputName { get; set; }

        [Required]
        public TState ResultantState { get; set; }

        public GuardConnector Guard { get; set; }
    }
}
