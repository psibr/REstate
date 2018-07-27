using REstate.Engine;
using System.Collections.Generic;

namespace REstate.Natural
{
    public class ConnectorContext
    {
        public INaturalSchematic Schematic { get; set; }

        public INaturalStateMachine Machine { get; set; }

        public Status<TypeState> Status { get; set; }

        public IReadOnlyDictionary<string, string> Settings { get; set; }
    }
}
