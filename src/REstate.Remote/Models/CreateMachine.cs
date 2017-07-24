using System.Collections.Generic;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class CreateMachineFromStoreRequest
    {
        [Key(0)]
        public string SchematicName { get; set; }

        [Key(1)]
        public IDictionary<string, string> Metadata { get; set; }
    }

    [MessagePackObject]
    public class CreateMachineFromSchematicRequest
    {
        [Key(0)]
        public byte[] SchematicBytes { get; set; }

        [Key(1)]
        public IDictionary<string, string> Metadata { get; set; }
    }

    [MessagePackObject]
    public class CreateMachineResponse
    {
        [Key(0)]
        public string MachineId { get; set; }
    }
}
