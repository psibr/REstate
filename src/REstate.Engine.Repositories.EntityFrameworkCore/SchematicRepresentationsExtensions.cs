using MessagePack;
using MessagePack.Resolvers;
using REstate.Schematics;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public static class SchematicRepresentationsExtensions
    {
        public static byte[] ToSchematicRepresentation<TState, TInput>(this Schematic<TState, TInput> schematic)
        {
            return MessagePackSerializer.Serialize(schematic, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block));
        }

        public static Schematic<TState, TInput> ToSchematic<TState, TInput>(this byte[] schematicRepresentation)
        {
            return MessagePackSerializer.Deserialize<Schematic<TState, TInput>>(
                schematicRepresentation,
                MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance).WithCompression(MessagePackCompression.Lz4Block));
        }
    }
}