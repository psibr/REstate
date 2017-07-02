using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class StateConfiguration<TState>
    {
        [Required]
        public TState Value { get; set; }
        public TState ParentState { get; set; }
        public string Description { get; set; }
        public Transition<TState>[] Transitions { get; set; }
        public EntryConnector OnEntry { get; set; }
    }
}
