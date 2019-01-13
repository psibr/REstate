using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine;

namespace REstate.Natural
{
    public interface INaturalStateEngine
    {
        Task<INaturalStateMachine> CreateMachineAsync(
            INaturalSchematic naturalSchematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default);

        Task<INaturalStateMachine> CreateMachineAsync(
            string schematicName,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default);

        Task<INaturalStateMachine> CreateMachineAsync(
            INaturalSchematic naturalSchematic,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default);

        Task<INaturalStateMachine> CreateMachineAsync(
            string schematicName,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<INaturalStateMachine>> BulkCreateMachinesAsync(
            INaturalSchematic naturalSchematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<INaturalStateMachine>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default);

        Task<INaturalStateMachine> GetMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default);

        INaturalStateMachine GetMachineReference(string machineId);

        Task<INaturalSchematic> GetSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default);

        Task<INaturalSchematic> StoreSchematicAsync(
            INaturalSchematic naturalSchematic,
            CancellationToken cancellationToken = default);

        Task<INaturalStateMachine> CreateMachineAsync<TNaturalSchematicFactory>(
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new();

        Task<INaturalStateMachine> CreateMachineAsync<TNaturalSchematicFactory>(
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new();
    }

    public class NaturalStateEngine : INaturalStateEngine
    {
        private readonly IAgent _agent;
        private readonly IStateEngine<TypeState, TypeState> _stateEngine;

        public NaturalStateEngine(IAgent agent, IStateEngine<TypeState, TypeState> stateEngine)
        {
            _agent = agent;
            _stateEngine = stateEngine;
        }

        public async Task<INaturalStateMachine> CreateMachineAsync(
            INaturalSchematic naturalSchematic,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            var machine = await _stateEngine
                .CreateMachineAsync(naturalSchematic, metadata, cancellationToken)
                .ConfigureAwait(false);

            return new NaturalStateMachine(machine);
        }

        public async Task<INaturalStateMachine> CreateMachineAsync(
            string schematicName,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            var machine = await _stateEngine
                .CreateMachineAsync(schematicName, metadata, cancellationToken)
                .ConfigureAwait(false);

            return new NaturalStateMachine(machine);
        }

        public async Task<INaturalStateMachine> CreateMachineAsync(
            INaturalSchematic naturalSchematic,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            var machine = await _stateEngine
                .CreateMachineAsync(naturalSchematic, machineId, metadata, cancellationToken)
                .ConfigureAwait(false);

            return new NaturalStateMachine(machine);
        }

        public async Task<INaturalStateMachine> CreateMachineAsync(
            string schematicName,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
        {
            var machine = await _stateEngine
                .CreateMachineAsync(schematicName, machineId, metadata, cancellationToken)
                .ConfigureAwait(false);

            return new NaturalStateMachine(machine);
        }

        public async Task<IEnumerable<INaturalStateMachine>> BulkCreateMachinesAsync(
            INaturalSchematic naturalSchematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            var machines = await _stateEngine
                .BulkCreateMachinesAsync(naturalSchematic, metadata, cancellationToken)
                .ConfigureAwait(false);

            return machines.Select(machine => new NaturalStateMachine(machine));
        }

        public async Task<IEnumerable<INaturalStateMachine>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default)
        {
            var machines = await _stateEngine
                .BulkCreateMachinesAsync(schematicName, metadata, cancellationToken)
                .ConfigureAwait(false);

            return machines.Select(machine => new NaturalStateMachine(machine));
        }

        public Task DeleteMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default) 
            => _stateEngine.DeleteMachineAsync(machineId, cancellationToken);

        public async Task<INaturalStateMachine> GetMachineAsync(
            string machineId,
            CancellationToken cancellationToken = default)
        {
            var machine = await _stateEngine
                .GetMachineAsync(machineId, cancellationToken)
                .ConfigureAwait(false);

            return new NaturalStateMachine(machine);
        }


        public INaturalStateMachine GetMachineReference(string machineId)
        {
            var machine = _stateEngine.GetMachineReference(machineId);

            return new NaturalStateMachine(machine);
        }

        public async Task<INaturalSchematic> GetSchematicAsync(
            string schematicName,
            CancellationToken cancellationToken = default)
        {
            var schematic = await _stateEngine
                .GetSchematicAsync(schematicName, cancellationToken)
                .ConfigureAwait(false);

            return new NaturalSchematic(schematic);
        }

        public async Task<INaturalSchematic> StoreSchematicAsync(
            INaturalSchematic naturalSchematic,
            CancellationToken cancellationToken = default)
        {
            var schematic = await _stateEngine
                .StoreSchematicAsync(naturalSchematic, cancellationToken)
                .ConfigureAwait(false);

            return new NaturalSchematic(schematic);
        }

        public async Task<INaturalStateMachine> CreateMachineAsync<TNaturalSchematicFactory>(
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            var schematic = _agent.ConstructSchematic<TNaturalSchematicFactory>();

            var machine = await CreateMachineAsync(
                schematic,
                machineId,
                metadata,
                cancellationToken).ConfigureAwait(false);

            return machine;
        }

        public async Task<INaturalStateMachine> CreateMachineAsync<TNaturalSchematicFactory>(
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            var schematic = _agent.ConstructSchematic<TNaturalSchematicFactory>();

            var machine = await CreateMachineAsync(
                schematic,
                metadata,
                cancellationToken).ConfigureAwait(false);

            return machine;
        }
    }
}
