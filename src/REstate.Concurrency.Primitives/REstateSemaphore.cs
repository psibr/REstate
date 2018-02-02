using System;
using System.Threading;
using System.Threading.Tasks;
using REstate.Engine;

namespace REstate.Concurrency.Primitives
{
    public class REstateSemaphore
        : IREstateSemaphore
    {
        public REstateSemaphore(IStateMachine<int, int> semaphoreMachine)
        {
            SemaphoreMachine = semaphoreMachine;
        }

        private IStateMachine<int, int> SemaphoreMachine { get; }

        public async Task<IDisposable> EnterAsync(CancellationToken cancellationToken = default)
        {
            var criticalSection =  new CriticalSection(SemaphoreMachine, cancellationToken);

            await criticalSection.EnterAsync();

            return criticalSection;
        }

        private class CriticalSection
            : IDisposable
        {
            public CriticalSection(
                IStateMachine<int, int> sempahoreStateMachine, 
                CancellationToken cancellationToken = default)
            {
                SempahoreStateMachine = sempahoreStateMachine;
                CancellationToken = cancellationToken;
            }

            public async Task EnterAsync()
            {
                await SempahoreStateMachine.SendAsync(1, CancellationToken);
            }

            private IStateMachine<int, int> SempahoreStateMachine { get; }
            private CancellationToken CancellationToken { get; }

            public void Dispose()
            {
                var mre = new ManualResetEventSlim();

                SempahoreStateMachine.SendAsync(-1, CancellationToken).GetAwaiter().OnCompleted(() => mre.Set());

                mre.Wait(CancellationToken.None);
            }
        }
    }
}
