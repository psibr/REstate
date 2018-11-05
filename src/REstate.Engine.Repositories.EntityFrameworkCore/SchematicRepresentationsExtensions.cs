using MessagePack;
using MessagePack.Resolvers;
using REstate.Schematics;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public static class SchematicRepresentationsExtensions
    {
        public static byte[] ToSchematicRepresentation<TState, TInput>(this Schematic<TState, TInput> schematic)
        {
            return LZ4MessagePackSerializer.Serialize(schematic, ContractlessStandardResolver.Instance);
        }

        public static Schematic<TState, TInput> ToSchematic<TState, TInput>(this byte[] schematicRepresentation)
        {
            return LZ4MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                schematicRepresentation,
                ContractlessStandardResolver.Instance);
        }
    }
}