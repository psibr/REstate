using MessagePack;
using MessagePack.Resolvers;

namespace REstate.Engine.Repositories.EntityFrameworkCore
{
    public static class StateRepresentationsExtensions
    {
        public static string ToStateRepresentation<TState>(TState state)
        {
            return MessagePackSerializer.ToJson(state, ContractlessStandardResolver.Instance);
        }

        public static TState ToState<TState>(string stateRepresentation)
        {
            return MessagePackSerializer.Deserialize<TState>(
                MessagePackSerializer.FromJson(stateRepresentation),
                ContractlessStandardResolver.Instance);
        }
    }
}