using System;
using System.ComponentModel.DataAnnotations;

namespace REstate.Engine.Repositories.InMemory
{
    internal class MachineStatus<TState>
    {
        [Required]
        public string SchematicName { get; set; }

        [Required]
        public TState State { get; set; }

        [Required]
        public Guid CommitTag { get; set; }

        [Required]
        public DateTime StateChangedDateTime { get; set; }

        public static implicit operator State<TState>(MachineStatus<TState> record)
        {
            return new State<TState>(record.State, record.CommitTag);
        }
    }
}
