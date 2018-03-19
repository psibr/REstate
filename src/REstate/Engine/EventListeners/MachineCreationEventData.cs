using System;
using System.Collections.Generic;

namespace REstate.Engine.EventListeners
{
    public class MachineCreationEventData<TState>
    {
        public MachineCreationEventData(Status<TState> initialStatus, IReadOnlyDictionary<string, string> metadata)
        {
            InitialStatus = initialStatus;
            Metadata = metadata;
        }

        public Status<TState> InitialStatus { get; }

        public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
