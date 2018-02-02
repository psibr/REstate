using System;
using System.Threading;
using System.Threading.Tasks;

namespace REstate.Concurrency.Primitives
{
    public interface IREstateMutex
    {
        Task<IDisposable> EnterAsync(CancellationToken cancellationToken = default);
    }
}
