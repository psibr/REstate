using REstate.Engine;
using REstate.Schematics;
using REstate.Schematics.Builder;
using REstate.Schematics.Builders;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace REstate
{
    public static class AgentExtensions
    {
        public static ISchematicBuilder<TState, TInput> CreateSchematic<TState, TInput>(
            this IAgent agent,
            string schematicName) => new SchematicBuilder<TState, TInput>(schematicName);

        public static ILocalAgent AsLocal(this IAgent agent) =>
            new LocalAgent(agent);
    }

    public static class TypeSchematicFactoryAgentExtensions
    {
        public static Schematic<TypeState, TypeState> ConstructSchematic<TTypeSchematicFactory>(this IAgent agent)
            where TTypeSchematicFactory : ITypeSchematicFactory, new()
        {
            return new TTypeSchematicFactory().BuildSchematic(agent);
        }

        public static Task<IStateMachine<TypeState, TypeState>> CreateMachineAsync<TTypeSchematicFactory>(
            this IAgent agent,
            string machineId,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TTypeSchematicFactory : ITypeSchematicFactory, new()
        {
            var schematic = agent.ConstructSchematic<TTypeSchematicFactory>();

            return agent.GetStateEngine<TypeState, TypeState>()
                .CreateMachineAsync(
                    schematic,
                    machineId,
                    metadata,
                    cancellationToken);
        }

        public static Task<IStateMachine<TypeState, TypeState>> CreateMachineAsync<TTypeSchematicFactory>(
            this IAgent agent,
            IDictionary<string, string> metadata = null,
            CancellationToken cancellationToken = default)
            where TTypeSchematicFactory : ITypeSchematicFactory, new()
        {
            var schematic = agent.ConstructSchematic<TTypeSchematicFactory>();

            return agent.GetStateEngine<TypeState, TypeState>()
                .CreateMachineAsync(
                    schematic,
                    metadata,
                    cancellationToken);
        }
    }
}
