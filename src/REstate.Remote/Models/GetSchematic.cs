using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class GetSchematicRequest
    {
        [Key(0)]
        public string SchematicName { get; set; }
    }

    [MessagePackObject]
    public class GetSchematicResponse
    {
        [Key(0)]
        public byte[] SchematicBytes { get; set; }
    }
}
