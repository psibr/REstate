using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class StateConfiguration<TState, TInput>
    {
        [Required]
        public TState Value { get; set; }
        public TState ParentState { get; set; }
        public string Description { get; set; }
        public Transition<TState, TInput>[] Transitions { get; set; }
        public EntryConnector<TInput> OnEntry { get; set; }
    }
}
