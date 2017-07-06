using System;
using MessagePack;

namespace REstate.Interop.Models
{
    [MessagePackObject]
    public class SendResponse
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(2)]
        public Guid CommitTag { get; set; }

        [Key(3)]
        public byte[] StateBytes { get; set; }
    }
}