using System;
using MessagePack;

namespace REstate.Interop.Models
{
    [MessagePackObject]
    public class SendRequest
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(1)]
        public byte[] InputBytes { get; set; }

        [Key(2)]
        public byte[] PayloadBytes { get; set; }

        [Key(3)]
        public Guid CommitTag { get; set; }
    }
}