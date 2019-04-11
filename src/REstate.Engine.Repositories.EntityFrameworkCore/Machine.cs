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

        public string SchematicName { get; set; }

        public byte[] SchematicBytes { get; set; }
        
        public List<StateBagEntry> StateBagEntries { get; set; }

        /// <summary>
        /// If Machine uses NaturalStateMachine flows, this property reflects the value of <see cref="TypeState.GetStateName"/>
        /// </summary>
        public string NaturalStateName { get; set; }
    }
}