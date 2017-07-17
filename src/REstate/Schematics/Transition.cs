using System.ComponentModel.DataAnnotations;

namespace REstate.Schematics
{
    public class Transition<TState, TInput>
        : ITransition<TState, TInput>
    {
        [Required]
        public TInput Input { get; set; }

        [Required]
        public TState ResultantState { get; set; }

        public GuardConnector Guard { get; set; }

        IGuard ITransition<TState, TInput>.Guard => Guard;
    }
}
