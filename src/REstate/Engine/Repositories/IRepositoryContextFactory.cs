using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories
{
    public interface IRepositoryContextFactory<TState, TInput>
    {
        Task<IEngineRepositoryContext<TState, TInput>> OpenContextAsync(CancellationToken cancellationToken = default);
    }
}
