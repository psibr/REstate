using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class StoreSchematicRequest
    {
        [Key(0)]
        public byte[] SchematicBytes { get; set; }
    }

    [MessagePackObject]
    public class StoreSchematicResponse
    {
        [Key(0)]
        public byte[] SchematicBytes { get; set; }
    }
}
