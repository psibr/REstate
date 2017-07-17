using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class GetMachineRequest
    {
        [Key(0)]
        public string MachineId { get; set; }
    }

    [MessagePackObject]
    public class GetMachineResponse
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(1)]
        public byte[] SchematicBytes { get; set; }
    }
}
