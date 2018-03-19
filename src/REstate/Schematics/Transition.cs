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

        public Precondition Precondition { get; set; }

        IPrecondition ITransition<TState, TInput>.Procondition => Precondition;
    }
}
