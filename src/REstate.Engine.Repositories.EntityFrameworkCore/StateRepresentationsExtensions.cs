using MessagePack;
using MessagePack.Resolvers;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public static class StateRepresentationsExtensions
    {
        public static string ToStateRepresentation<TState>(this TState state)
        {
            return MessagePackSerializer.ToJson(state, ContractlessStandardResolver.Instance);
        }

        public static TState ToState<TState>(this string stateRepresentation)
        {
            return MessagePackSerializer.Deserialize<TState>(
                MessagePackSerializer.FromJson(stateRepresentation),
                ContractlessStandardResolver.Instance);
        }
    }
}