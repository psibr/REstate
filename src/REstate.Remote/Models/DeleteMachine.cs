using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace REstate.Remote.Models
{
    [MessagePackObject]
    public class DeleteMachineRequest
    {
        [Key(0)]
        public string MachineId { get; set; }
    }
}
