using MessagePack;
using MessagePack.Resolvers;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public static class StateRepresentationsExtensions
    {
        public static string ToStateRepresentation<TState>(this TState state)
        {
            return MessagePackSerializer.SerializeToJson(state, MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));
        }

        public static TState ToState<TState>(this string stateRepresentation)
        {
            return MessagePackSerializer.Deserialize<TState>(
                MessagePackSerializer.ConvertFromJson(stateRepresentation), 
                MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));
        }
    }
}