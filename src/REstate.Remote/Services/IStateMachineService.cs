using MagicOnion;
using MessagePack;
using REstate.Remote.Models;

namespace REstate.Remote.Services
{
    public interface IStateMachineService
        : IService<IStateMachineService>
    {
        UnaryResult<SendResponse> SendAsync(SendRequest request);

        UnaryResult<SendResponse> SendWithPayloadAsync(SendWithPayloadRequest request);

        UnaryResult<StoreSchematicResponse> StoreSchematicAsync(StoreSchematicRequest request);

        UnaryResult<GetMachineSchematicResponse> GetMachineSchematicAsync(GetMachineSchematicRequest request);

        UnaryResult<GetMachineMetadataResponse> GetMachineMetadataAsync(GetMachineMetadataRequest request);

        UnaryResult<GetSchematicResponse> GetSchematicAsync(GetSchematicRequest request);

        UnaryResult<Nil> DeleteMachineAsync(DeleteMachineRequest request);

        UnaryResult<CreateMachineResponse> CreateMachineFromStoreAsync(CreateMachineFromStoreRequest request);

        UnaryResult<CreateMachineResponse> CreateMachineFromSchematicAsync(CreateMachineFromSchematicRequest request);

        UnaryResult<BulkCreateMachineResponse> BulkCreateMachineFromStoreAsync(BulkCreateMachineFromStoreRequest request);

        UnaryResult<BulkCreateMachineResponse> BulkCreateMachineFromSchematicAsync(BulkCreateMachineFromSchematicRequest request);
    }
}