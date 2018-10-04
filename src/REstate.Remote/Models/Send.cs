using System;
using System.Collections.Generic;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class SendWithPayloadRequest
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(1)]
        public byte[] InputBytes { get; set; }

        [Key(2)]
        public byte[] PayloadBytes { get; set; }

        [Key(3)]
        public long? CommitNumber { get; set; }

        [Key(4)]
        public IDictionary<string, string> StateBag { get; set; }
    }

    [MessagePackObject]
    public class SendRequest
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(1)]
        public byte[] InputBytes { get; set; }

        [Key(2)]
        public long? CommitNumber { get; set; }

        [Key(3)]
        public IDictionary<string, string> StateBag { get; set; }
    }

    [MessagePackObject]
    public class SendResponse
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(2)]
        public long CommitNumber { get; set; }

        [Key(3)]
        public byte[] StateBytes { get; set; }

        [Key(4)]
        public DateTimeOffset UpdatedTime { get; set; }

        [Key(5)]
        public IDictionary<string, string> StateBag { get; set; }
    }
}
