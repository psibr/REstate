using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace REstate.Connectors.Remote
{
    public interface IRemoteConnector
        : IService<IRemoteConnector>
    {
        UnaryResult<Nil> InvokeAsync();

        //Task InvokeAsync<TPayload>(
        //    ISchematic<TState, TInput> schematic,
        //    IStateMachine<TState, TInput> machine,
        //    Status<TState> status,
        //    InputParameters<TInput, TPayload> inputParameters,
        //    IReadOnlyDictionary<string, string> connectorSettings,
        //    CancellationToken cancellationToken = default);
    }

    [MessagePackObject]
    public class InvokeRequest
    {
        [Key(0)]
        public string MachineId { get; set; }

        [Key(1)]
        public byte[] SchematicBytes { get; set; }

        [Key(2)]
        public long CommitNumber { get; set; }

        [Key(3)]
        public byte[] StateBytes { get; set; }

        [Key(4)]
        public byte[] InputBytes { get; set; }

        [Key(5)]
        public byte[] PayloadBytes { get; set; }

        [Key(6)]
        public IDictionary<string, string> ConnectorSettings { get; set; }
    }

    public class RemoteConnector
        : ServiceBase<IRemoteConnector>
    {

    }
}
