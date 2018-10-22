using System;
using System.Collections.Generic;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public class Machine
    {
        public string MachineId { get; set; }

        public long CommitNumber { get; set; }

        public DateTimeOffset UpdatedTime { get; set; }

        public string StateJson { get; set; }

        public List<MetadataEntry> MetadataEntries { get; set; }

        public byte[] SchematicBytes { get; set; }
        
        public List<StateBagEntry> StateBagEntries { get; set; }
    }
}