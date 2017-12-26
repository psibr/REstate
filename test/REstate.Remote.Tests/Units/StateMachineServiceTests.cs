using MessagePack;
using MessagePack.Resolvers;
using Moq;
using REstate.Remote.Models;
using REstate.Remote.Services;
using REstate.Schematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MagicOnion.Server;
using Xunit;

namespace REstate.Remote.Tests.Units
{
    // method name, state type, input type, payload type
    using MethodKey = ValueTuple<string, Type, Type, Type>;

    public class StateMachineServiceTests
    {
        private readonly Mock<StateMachineService> _stateMachineServiceMock;
        private readonly Mock<IStateMachineServiceLocalAdapter> _localAdapterMock;

        public StateMachineServiceTests()
        {
            _localAdapterMock = new Mock<IStateMachineServiceLocalAdapter>(MockBehavior.Strict);

            _stateMachineServiceMock = new Mock<StateMachineService>(
                MockBehavior.Strict, 
                new ConcurrentDictionary<MethodKey, Delegate>(), 
                _localAdapterMock.Object);

            _stateMachineServiceMock
                .Setup(_ => _.GetGenericsFromHeaders())
                .Returns(new[] { typeof(string), typeof(string) });

            _stateMachineServiceMock
                .Setup(_ => _.GetCallCancellationToken())
                .Returns(CancellationToken.None);
        }

        [Fact]
        public void CanBuildServerServiceDefinition()
        {
            var service = MagicOnionEngine.BuildServerServiceDefinition(
                targetTypes: new[]
                {
                    typeof(StateMachineService)
                },
                option: new MagicOnionOptions(
                    isReturnExceptionStackTraceInErrorDetail: true));

            Assert.NotNull(service);
        }

        [Fact]
        public async Task SendAsync()
        {
            // Arrange
            var machineId = "some value";
            var input = "some input";
            var inputBytes = MessagePackSerializer.Serialize(input, ContractlessStandardResolver.Instance);
            var stateBytes = new byte[50];
            var commitTag = new Guid();
            var updatedCommitTag = new Guid();
            var updatedTime = DateTime.Now;

            _localAdapterMock
                .Setup(_ => _.SendAsync<string, string>(
                    It.Is<string>(it => it == machineId),
                    It.Is<string>(it => it == input),
                    It.Is<Guid>(it => it == commitTag),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendResponse
                {
                    MachineId = machineId,
                    CommitTag = updatedCommitTag,
                    StateBytes = stateBytes,
                    UpdatedTime = updatedTime
                });

            // Act
            var response = await _stateMachineServiceMock.Object
                .SendAsync(new SendRequest
                {
                    MachineId = machineId,
                    CommitTag = commitTag,
                    InputBytes = inputBytes
                });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(machineId, response.MachineId);
            Assert.Equal(updatedCommitTag, response.CommitTag);
            Assert.Equal(stateBytes, response.StateBytes);
            Assert.Equal(updatedTime, response.UpdatedTime);
        }

        [Fact]
        public async Task SendWithPayloadAsync()
        {
            // Arrange
            var machineId = "some machine";
            var input = "some input";
            var payload = "some payload";
            var inputBytes = MessagePackSerializer.Serialize(input, ContractlessStandardResolver.Instance);
            var payloadBytes = MessagePackSerializer.Typeless.Serialize(payload);
            var commitTag = Guid.NewGuid();
            var stateBytes = new byte[50];
            var updatedCommitTag = Guid.NewGuid();
            var updatedTime = DateTime.Now;

            _localAdapterMock
                .Setup(_ => _.SendWithPayloadAsync<string, string, string>(
                    It.Is<string>(it => it == machineId),
                    It.Is<string>(it => it == input),
                    It.Is<string>(it => it == payload),
                    It.Is<Guid>(it => it == commitTag),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendResponse
                {
                    MachineId = machineId,
                    CommitTag = updatedCommitTag,
                    StateBytes = stateBytes,
                    UpdatedTime = updatedTime
                });

            // Act
            var response = await _stateMachineServiceMock.Object
                .SendWithPayloadAsync(new SendWithPayloadRequest
                {
                    MachineId = machineId,
                    InputBytes = inputBytes,
                    PayloadBytes = payloadBytes,
                    CommitTag = commitTag
                });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(machineId, response.MachineId);
            Assert.Equal(stateBytes, response.StateBytes);
            Assert.Equal(updatedCommitTag, response.CommitTag);
            Assert.Equal(updatedTime, response.UpdatedTime);
        }

        [Fact]
        public async Task StoreSchematicAsync()
        {
            // Arrange
            var schematic = new Schematic<string, string>{ SchematicName = "schematic name" };
            var schematicBytes = MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);

            _localAdapterMock
                .Setup(_ => _.StoreSchematicAsync(
                    It.Is<Schematic<string, string>>(it => it.SchematicName == schematic.SchematicName),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StoreSchematicResponse
                {
                    SchematicBytes = schematicBytes
                });

            // Act
            var response = await _stateMachineServiceMock.Object
                .StoreSchematicAsync(new StoreSchematicRequest
                {
                    SchematicBytes = schematicBytes
                });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(schematicBytes, response.SchematicBytes);
        }

        [Fact]
        public async Task GetMachineSchematicAsync()
        {
            // Arrange
            var machineId = "some value";
            var schematicBytes = new byte[50];

            _localAdapterMock
                .Setup(_ => _.GetMachineSchematicAsync<string, string>(
                    It.Is<string>(it => it == machineId), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMachineSchematicResponse
                {
                    MachineId = machineId,
                    SchematicBytes = schematicBytes
                });
            
            // Act
            var response = await _stateMachineServiceMock.Object
                .GetMachineSchematicAsync(new GetMachineSchematicRequest
                {
                    MachineId = machineId
                });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(machineId, response.MachineId);
            Assert.Equal(schematicBytes, response.SchematicBytes);
        }

        [Fact]
        public async Task GetMachineMetadataAsync()
        {
            // Arrange
            var machineId = "some value";
            var key = "key";
            var value = "value";
            var metadata = new Dictionary<string, string> { { key, value } };

            _localAdapterMock
                .Setup(_ => _.GetMachineMetadataAsync<string, string>(
                    It.Is<string>(it => it == machineId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetMachineMetadataResponse
                {
                    Metadata = metadata,
                    MachineId = machineId
                });

            // Act
            var response = await _stateMachineServiceMock.Object
                .GetMachineMetadataAsync(new GetMachineMetadataRequest
                {
                    MachineId = machineId
                });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(value, response.Metadata[key]);
        }

        [Fact]
        public async Task CreateMachineFromStoreAsync()
        {
            // Arrange
            var schematicName = "schematic name";
            var key = "key";
            var value = "value";
            var metadata = new Dictionary<string, string> { { key, value } };
            var machineId = "some machineId";

            _localAdapterMock
                .Setup(_ => _.CreateMachineFromStoreAsync<string, string>(
                    It.Is<string>(it => it == schematicName),
                    machineId,
                    It.Is<IDictionary<string, string>>(it => it[key] == metadata[key]),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateMachineResponse
                {
                    MachineId = machineId
                });

            // Act
            var response = await _stateMachineServiceMock.Object
                .CreateMachineFromStoreAsync(new CreateMachineFromStoreRequest
                {
                    SchematicName = schematicName,
                    Metadata = metadata,
                    MachineId = machineId
                });

            //Assert
            Assert.NotNull(response);
            Assert.Equal(machineId, response.MachineId);
        }

        [Fact]
        public async Task CreateMachineFromSchematicAsync()
        {
            // Arrange
            var schematic = new Schematic<string, string>
            {
                SchematicName = "schematic name",
                InitialState = "initial state",
                States = new State<string, string>[] { new State<string, string> { Value = "initial state" } }
            };
            var schematicBytes = MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);
            var key = "key";
            var value = "value";
            var metadata = new Dictionary<string, string> { { key, value } };
            var machineId = "some machineId";

            _localAdapterMock
                .Setup(_ => _.CreateMachineFromSchematicAsync(
                    It.Is<Schematic<string, string>>(it => it.SchematicName == schematic.SchematicName),
                    machineId,
                    It.Is<IDictionary<string, string>>(it => it[key] == metadata[key]),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateMachineResponse
                {
                    MachineId = machineId
                });

            // Act
            var response = await _stateMachineServiceMock.Object
                .CreateMachineFromSchematicAsync(new CreateMachineFromSchematicRequest
                {
                    SchematicBytes = schematicBytes,
                    Metadata = metadata,
                    MachineId = machineId
                });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(machineId, response.MachineId);
        }

        [Fact]
        public async Task BulkCreateMachineFromStoreAsync()
        {
            // Arrange
            var schematicName = "schematic name";
            var metadataEnumerable = new List<Dictionary<string, string>>();
            var firstKey = "first key";
            var firstValue = "first value";
            var firstMetadataEntry = new Dictionary<string, string> { { firstKey, firstValue } };
            var secondKey = "second key";
            var secondValue = "second value";
            var secondMetadataEntry = new Dictionary<string, string> { { secondKey, secondValue } };
            metadataEnumerable.Add(firstMetadataEntry);
            metadataEnumerable.Add(secondMetadataEntry);

            _localAdapterMock
                .Setup(_ => _.BulkCreateMachineFromStoreAsync<string, string>(
                    It.Is<string>(it => it == schematicName),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _stateMachineServiceMock.Object
                .BulkCreateMachineFromStoreAsync(new BulkCreateMachineFromStoreRequest
                {
                    SchematicName = schematicName,
                    Metadata = metadataEnumerable
                });

            // Assert
            _localAdapterMock.Verify(
                expression: _ => _.BulkCreateMachineFromStoreAsync<string, string>(
                    schematicName,
                    metadataEnumerable,
                    CancellationToken.None),
                times: Times.Once());
        }

        [Fact]
        public async Task BulkCreateMachineFromSchematicAsync()
        {
            // Arrange
            var schematic = new Schematic<string, string>
            {
                SchematicName = "schematic name",
                InitialState = "initial state",
                States = new[] { new State<string, string> { Value = "initial state" } }
            };
            var schematicBytes = MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);
            var metadataEnumerable = new List<Dictionary<string, string>>();
            var firstKey = "first key";
            var firstValue = "first value";
            var firstMetadataEntry = new Dictionary<string, string> { { firstKey, firstValue } };
            var secondKey = "second key";
            var secondValue = "second value";
            var secondMetadataEntry = new Dictionary<string, string> { { secondKey, secondValue } };
            metadataEnumerable.Add(firstMetadataEntry);
            metadataEnumerable.Add(secondMetadataEntry);

            _localAdapterMock
                .Setup(_ => _.BulkCreateMachineFromSchematicAsync<string, string>(
                    It.Is<Schematic<string, string>>(it => it.SchematicName == schematic.SchematicName),
                    It.IsAny<IEnumerable<IDictionary<string, string>>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _stateMachineServiceMock.Object
                .BulkCreateMachineFromSchematicAsync(new BulkCreateMachineFromSchematicRequest
                {
                    SchematicBytes = schematicBytes,
                    Metadata = metadataEnumerable
                });

            // Assert
            // Note: first parameter has to be checked for It.Is, other parameters CANNOT or the verify will fail
            _localAdapterMock.Verify(
                expression: _ => _.BulkCreateMachineFromSchematicAsync(
                    It.Is<Schematic<string, string>>(it => it.SchematicName == schematic.SchematicName), 
                    metadataEnumerable, 
                    CancellationToken.None),
                times: Times.Once());
        }

        [Fact]
        public async Task GetSchematicAsync()
        {
            // Arrange
            var schematicName = "schematic name";
            var schematic = new Schematic<string, string> { SchematicName = "schematic name" };
            var schematicBytes = MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);

            _localAdapterMock
                .Setup(_ => _.GetSchematicAsync<string, string>(
                    It.Is<string>(it => it == schematicName),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetSchematicResponse
                {
                    SchematicBytes = schematicBytes
                });

            // Act
            var response = await _stateMachineServiceMock.Object
                .GetSchematicAsync(new GetSchematicRequest
                {
                    SchematicName = schematicName
                });

            // Assert
            Assert.NotNull(response);
            Assert.Equal(schematicBytes, response.SchematicBytes);
        }

        [Fact]
        public async Task DeleteMachineAsync()
        {
            // Arrange
            var machineId = "some machine";

            // This one has to return a completed task or you get a null ref exception
            _localAdapterMock
                .Setup(_ => _.DeleteMachineAsync<string, string>(
                    It.Is<string>(it => it == machineId),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _stateMachineServiceMock.Object
                .DeleteMachineAsync(new DeleteMachineRequest
                {
                    MachineId = machineId
                });

            // Assert
            _localAdapterMock.Verify(
                expression: _ => _.DeleteMachineAsync<string, string>(machineId, CancellationToken.None),
                times: Times.Once());
        }
    }
}
