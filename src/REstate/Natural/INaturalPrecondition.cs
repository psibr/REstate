using System.Threading;
using System.Threading.Tasks;
using REstate.Engine.Connectors;

namespace REstate.Natural
{
    public interface INaturalPrecondition<TSignal> 
        : IPrecondition<TypeState, TypeState>
    {
        Task<bool> ValidateAsync(
            ConnectorContext context,
            TSignal signal,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}