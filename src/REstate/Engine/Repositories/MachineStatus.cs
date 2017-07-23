using System;
using System.ComponentModel.DataAnnotations;
using REstate.Schematics;

namespace REstate.Engine.Repositories
{
    public class MachineStatus<TState, TInput>
    {
        public string MachineId { get; set; }

        public Schematic<TState, TInput> Schematic { get; set; }

        public TState State { get; set; }

        public Guid CommitTag { get; set; }

        public DateTime StateChangedDateTime { get; set; }

        public static implicit operator Status<TState>(MachineStatus<TState, TInput> record)
        {
            return new Status<TState>(record.State, record.CommitTag);
        }
    }
}
