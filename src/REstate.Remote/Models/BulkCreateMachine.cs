using System.Collections.Generic;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class BulkCreateMachineFromStoreRequest
    {
        [Key(0)]
        public string SchematicName { get; set; }

        [Key(1)]
        public IEnumerable<IDictionary<string, string>> Metadata { get; set; }
    }

    [MessagePackObject]
    public class BulkCreateMachineFromSchematicRequest
    {
        [Key(0)]
        public byte[] SchematicBytes { get; set; }

        [Key(1)]
        public IEnumerable<IDictionary<string, string>> Metadata { get; set; }
    }
}