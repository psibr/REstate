using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.Natural;
using REstate.Natural.Schematics.Builders;

namespace REstate
{
    public static class NaturalFactoryAgentExtensions
    {
        public static INaturalStateEngine GetNaturalStateEngine(this IAgent agent)
        {
            return new NaturalStateEngine(agent.GetStateEngine<TypeState, TypeState>());
        }

        public static INaturalSchematic ConstructSchematic<TNaturalSchematicFactory>(this IAgent agent)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            return new TNaturalSchematicFactory().BuildSchematic(new NaturalSchematicBuilder(agent));
        }

        public static INaturalSchematic ConstructSchematic(this IAgent agent, INaturalSchematicFactory naturalSchematicFactory)
        {
            return naturalSchematicFactory.BuildSchematic(new NaturalSchematicBuilder(agent));
        }

        public static async Task<INaturalStateMachine> CreateMachineAsync<TNaturalSchematicFactory>(
            this IAgent agent,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            var schematic = agent.ConstructSchematic<TNaturalSchematicFactory>();

            var machine = await agent
                .GetNaturalStateEngine()
                .CreateMachineAsync(
                    schematic,
                    machineId,
                    metadata,
                    cancellationToken).ConfigureAwait(false);

            return machine;
        }

        public static async Task<INaturalStateMachine> CreateMachineAsync<TNaturalSchematicFactory>(
            this IAgent agent,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TNaturalSchematicFactory : INaturalSchematicFactory, new()
        {
            var schematic = agent.ConstructSchematic<TNaturalSchematicFactory>();

            var machine = await agent
                .GetNaturalStateEngine()
                .CreateMachineAsync(
                    schematic,
                    metadata,
                    cancellationToken).ConfigureAwait(false);

            return machine;
        }
    }
}