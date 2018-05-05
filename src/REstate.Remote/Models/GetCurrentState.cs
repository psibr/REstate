using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class GetCurrentStateRequest
    {
        [Key(0)]
        public string MachineId { get; set; }
    }

    [MessagePackObject]
    public class GetCurrentStateResponse
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(2)]
        public long CommitNumber { get; set; }

        [Key(3)]
        public byte[] StateBytes { get; set; }

        [Key(4)]
        public DateTimeOffset UpdatedTime { get; set; }
    }
}
