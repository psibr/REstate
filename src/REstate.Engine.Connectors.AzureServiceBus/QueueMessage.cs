using System;

namespace REstate.Engine.Connectors.AzureServiceBus
{
    public class QueueMessage<TState, TInput, TPayload>
    {
        public TState State { get; set; }
        public Guid CommitTag { get; set; }
        public string MachineId { get; set; }
        public DateTimeOffset UpdatedTime { get; set; }

        public TInput Input { get; set; }
        public TPayload Payload { get; set; }
    }
}