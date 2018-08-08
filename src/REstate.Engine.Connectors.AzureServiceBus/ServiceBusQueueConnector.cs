using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Azure.ServiceBus;
using REstate.Schematics;

namespace REstate.Engine.Connectors.AzureServiceBus
{
    public class ServiceBusQueueConfiguration
        : IConnectorConfiguration
    {
        public ServiceBusQueueConfiguration(string identifier, string connectionString)
        {
            Identifier = identifier;
            ConnectionString = connectionString;
        }

        public string Identifier { get; }

        public string ConnectionString { get; }
    }

    /// <summary>
    /// Provides capability to place messages on Azure Service Bus Queues when entering a state.
    /// </summary>
    public class ServiceBusQueueConnector<TState, TInput>
        : IAction<TState, TInput>
    {
        private readonly IEnumerable<ServiceBusQueueConfiguration> _configurations;

        public ServiceBusQueueConnector(IEnumerable<ServiceBusQueueConfiguration> configurations)
        {
            _configurations = configurations;
        }

        public async Task InvokeAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status, 
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            var configuration = _configurations.FindConfiguration(schematic, status);

            var messageBytes = CreateMessageBytes(
                schematic,
                machine,
                status,
                inputParameters,
                connectorSettings,
                cancellationToken);

            var queueClient = new QueueClient(
                new ServiceBusConnectionStringBuilder(configuration.ConnectionString),
                ReceiveMode.PeekLock,
                RetryPolicy.Default);

            await queueClient.SendAsync(new Message(messageBytes));
        }

        public virtual byte[] CreateMessageBytes<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default)
        {
            var message = new QueueMessage<TState, TInput, TPayload>
            {
                State = status.State,
                CommitNumber = status.CommitNumber,
                MachineId = status.MachineId,
                UpdatedTime = status.UpdatedTime,
                Input = inputParameters.Input,
                Payload = inputParameters.Payload
            };

            return LZ4MessagePackSerializer.Serialize(message, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }
    }
}
