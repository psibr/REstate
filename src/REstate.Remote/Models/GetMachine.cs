using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class GetMachineSchematicRequest
    {
        [Key(0)]
        public string MachineId { get; set; }
    }

    [MessagePackObject]
    public class GetMachineSchematicResponse
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(1)]
        public byte[] SchematicBytes { get; set; }
    }
}
