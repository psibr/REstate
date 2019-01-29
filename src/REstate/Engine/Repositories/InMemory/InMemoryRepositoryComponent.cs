using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using REstate.IoC;

namespace REstate.Engine.Repositories.InMemory
{
    public class InMemoryRepositoryComponent
        : IComponent
    {
        public void Register(IRegistrar registrar)
        {
            registrar.Register(typeof(IRepositoryContextFactory<,>), typeof(InMemoryRepositoryContextFactory<,>));
        }
    }
}
