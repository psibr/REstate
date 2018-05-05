using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Remote.Models;
using REstate.Schematics;

namespace REstate.Remote.Services
{
    internal interface IStateMachineServiceLocalAdapter
    {
        Task<BulkCreateMachineResponse> BulkCreateMachineFromSchematicAsync<TState, TInput>(Schematic<TState, TInput> schematic, IEnumerable<IDictionary<string, string>> metadata, CancellationToken cancellationToken = default);
        Task<BulkCreateMachineResponse> BulkCreateMachineFromStoreAsync<TState, TInput>(string schematicName, IEnumerable<IDictionary<string, string>> metadata, CancellationToken cancellationToken = default);
        Task<CreateMachineResponse> CreateMachineFromSchematicAsync<TState, TInput>(Schematic<TState, TInput> schematic, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);
        Task<CreateMachineResponse> CreateMachineFromStoreAsync<TState, TInput>(string schematicName, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);
        Task DeleteMachineAsync<TState, TInput>(string machineId, CancellationToken cancellationToken);
        Task<GetMachineMetadataResponse> GetMachineMetadataAsync<TState, TInput>(string machineId, CancellationToken cancellationToken = default);
        Task<GetMachineSchematicResponse> GetMachineSchematicAsync<TState, TInput>(string machineId, CancellationToken cancellationToken = default);
        Task<GetSchematicResponse> GetSchematicAsync<TState, TInput>(string schematicName, CancellationToken cancellationToken);
        Task<GetCurrentStateResponse> GetCurrentStateAsync<TState, TInput>(string machineId, CancellationToken cancellationToken = default);
        Task<SendResponse> SendAsync<TState, TInput>(string machineId, TInput input, long? commitNumber, CancellationToken cancellationToken = default);
        Task<SendResponse> SendWithPayloadAsync<TState, TInput, TPayload>(string machineId, TInput input, TPayload payload, long? commitNumber, CancellationToken cancellationToken = default);
        Task<StoreSchematicResponse> StoreSchematicAsync<TState, TInput>(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default);
    }
}
