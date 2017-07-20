using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryRepositoryContextFactory<TState, TInput>
        : IRepositoryContextFactory<TState, TInput>
    {
        public Task<IEngineRepositoryContext<TState, TInput>> OpenContextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult<IEngineRepositoryContext<TState, TInput>>(new EngineRepositoryContext<TState, TInput>());
        }
    }
}
