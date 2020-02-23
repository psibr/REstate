using System.Threading;
using System.Threading.Tasks;
using REstate.Engine.Connectors;

namespace REstate.Natural
{
    public interface INaturalAction<TSignal>
        : IAction<TypeState, TypeState>
    {
        Task InvokeAsync(
            ConnectorContext context,
            TSignal signal,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}