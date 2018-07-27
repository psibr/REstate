using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine;
using REstate.Natural;
using REstate.Schematics;

namespace REstate
{
    public static class NaturalFactoryAgentExtensions
    {
        public static INaturalSchematic ConstructSchematic<TNaturalSchematicFactory>(this IAgent agent)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            return new TNaturalSchematicFactory().BuildSchematic(agent);
        }

        public static async Task<INaturalStateMachine> CreateNaturalMachineAsync<TNaturalSchematicFactory>(
            this IAgent agent,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            var schematic = agent.ConstructSchematic<TNaturalSchematicFactory>();

            var machine = await agent.GetStateEngine<TypeState, TypeState>()
                .CreateMachineAsync(
                    schematic,
                    machineId,
                    metadata,
                    cancellationToken).ConfigureAwait(false);

            return new NaturalStateMachine(machine);
        }

        public static async Task<INaturalStateMachine> CreateNaturalMachineAsync<TNaturalSchematicFactory>(
            this IAgent agent,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            var schematic = agent.ConstructSchematic<TNaturalSchematicFactory>();

            var machine = await agent.GetStateEngine<TypeState, TypeState>()
                .CreateMachineAsync(
                    schematic,
                    metadata,
                    cancellationToken).ConfigureAwait(false);

            return new NaturalStateMachine(machine);
        }
    }
}