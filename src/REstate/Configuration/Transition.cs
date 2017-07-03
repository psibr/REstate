using System;
using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class Transition<TState, TInput>
    {
        [Required]
        public TInput Input { get; set; }

        [Required]
        public TState ResultantState { get; set; }

        public GuardConnector Guard { get; set; }
    }
}
