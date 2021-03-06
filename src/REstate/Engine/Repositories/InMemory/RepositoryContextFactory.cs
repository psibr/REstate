﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryRepositoryContextFactory<TState, TInput>
        : IRepositoryContextFactory<TState, TInput>
    {
        private readonly Lazy<IEngineRepositoryContext<TState, TInput>> repositoryContextLazy 
            = new Lazy<IEngineRepositoryContext<TState, TInput>>(() 
                => new EngineRepositoryContext<TState, TInput>(), true);

        public Task<IEngineRepositoryContext<TState, TInput>> OpenContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(repositoryContextLazy.Value);
        }
    }
}
