using System.Collections.Generic;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class GetMachineMetadataRequest
    {
        [Key(0)]
        public string MachineId { get; set; }
    }

    [MessagePackObject]
    public class GetMachineMetadataResponse
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(1)]
        public IDictionary<string, string> Metadata { get; set; }
    }
}
