using System;
using System.ComponentModel.DataAnnotations;
using REstate.Schematics;

namespace REstate.Engine.Repositories
{
    public class MachineStatus<TState, TInput>
    {
        public Schematic<TState, TInput> Schematic { get; set; }

        public TState State { get; set; }

        public Guid CommitTag { get; set; }

        public DateTime StateChangedDateTime { get; set; }

        public static implicit operator State<TState>(MachineStatus<TState, TInput> record)
        {
            return new State<TState>(record.State, record.CommitTag);
        }
    }
}
