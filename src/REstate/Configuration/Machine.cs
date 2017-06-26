using REstate.Engine;
using System;
using System.ComponentModel.DataAnnotations;

namespace REstate.Configuration
{
    public class Machine
    {
        [Required]
        public string SchematicName { get; set; }

        [Required]
        public string StateName { get; set; }

        [Required]
        public string CommitTag { get; set; }

        [Required]
        public DateTime StateChangedDateTime { get; set; }

        public static implicit operator State(Machine record)
        {
            return new State(record.StateName, Guid.Parse(record.CommitTag));
        }
    }
}
