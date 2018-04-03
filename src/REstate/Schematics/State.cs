using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace REstate.Schematics
{
    public class State<TState, TInput>
        : IState<TState, TInput>
    {
        [Required]
        public TState Value { get; set; }
        public TState ParentState { get; set; }
        public string Description { get; set; }

        public Transition<TState, TInput>[] Transitions { get; set; }
        public Action<TInput> Action { get; set; }
        public Precondition Precondition { get; set; }

        IDictionary<TInput, ITransition<TState, TInput>> IState<TState, TInput>.Transitions =>
            Transitions.ToDictionary(
                t => t.Input,
                t => (ITransition<TState, TInput>)t);

        IAction<TInput> IState<TState, TInput>.Action => Action;

        IPrecondition IState<TState, TInput>.Precondition => Precondition;
    }
}
