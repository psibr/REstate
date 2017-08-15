using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace REstate.Engine.Repositories.Redis
{
    [MessagePackObject]
    public class RedisMachineStatus<TState, TInput>
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(1)]
        public string SchematicHash { get; set; }

        [Key(2)]
        public TState State { get; set; }

        [Key(3)]
        public Guid CommitTag { get; set; }

        [Key(4)]
        public DateTimeOffset UpdatedTime { get; set; }

        [Key(5)]
        public IDictionary<string, string> Metadata { get; set; }
    }
}
