using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using REstate.Concurrency.Primitives;
using REstate.Schematics;

// ReSharper disable once CheckNamespace
namespace REstate
{
    public static class ConcurrencyPrimitiveExtensions
    {
        /// <summary>
        /// Creates a new mutex (lock) using Machine state as the synchronization root.
        /// </summary>
        /// <param name="restateAgent">The agent to use to build the mutex</param>
        /// <param name="retryLimit">The number of attempts to try obtaining a lock; if null then no limit</param>
        /// <param name="cancellationToken">The cancellation token for building the Machine</param>
        /// <returns>A new mutex</returns>
        public static Task<IREstateMutex> CreateLockAsync(
            this IAgent restateAgent,
            int? retryLimit = null,
            CancellationToken cancellationToken = default)
        {
            return CreateMutexAsync(restateAgent, retryLimit, cancellationToken);
        }

        /// <summary>
        /// Creates a new mutex (lock) using Machine state as the synchronization root.
        /// </summary>
        /// <param name="restateAgent">The agent to use to build the mutex</param>
        /// <param name="retryLimit">The number of attempts to try obtaining a lock; if null then no limit</param>
        /// <param name="cancellationToken">The cancellation token for building the Machine</param>
        /// <returns>A new mutex</returns>
        public static async Task<IREstateMutex> CreateMutexAsync(
            this IAgent restateAgent,
            int? retryLimit = null,
            CancellationToken cancellationToken = default)
        {
            var mutexSchematic = CreateSemaphoreSchematic(restateAgent, 1, retryLimit);

            var mutexMachine = await restateAgent.GetStateEngine<int, int>()
                .CreateMachineAsync(mutexSchematic, null, cancellationToken);

            return new REstateSemaphore(mutexMachine);
        }

        /// <summary>
        /// Creates a new semaphore using Machine state as the synchronization root.
        /// </summary>
        /// <param name="restateAgent">The agent to use to build the semaphore</param>
        /// <param name="slots">The number or available slots in the sempahore</param>
        /// <param name="retryLimit">The number of attempts to try obtaining a slot; if null then no limit</param>
        /// <param name="cancellationToken">The cancellation token for building the Machine</param>
        /// <returns>A new semaphore</returns>
        public static async Task<IREstateSemaphore> CreateSemaphoreAsync(
            this IAgent restateAgent,
            int slots,
            int? retryLimit = null,
            CancellationToken cancellationToken = default)
        {
            if (slots < 1) throw new ArgumentOutOfRangeException(nameof(slots), "Sempahores must have at least one slot.");

            var sempahoreSchematic = CreateSemaphoreSchematic(restateAgent, slots, retryLimit);

            var sempahoreMachine = await restateAgent.GetStateEngine<int, int>()
                .CreateMachineAsync(sempahoreSchematic, null, cancellationToken);

            return new REstateSemaphore(sempahoreMachine);
        }

        /// <summary>
        /// Creates a Schematic that represents a semaphore with n slots
        /// </summary>
        /// <remarks>
        /// The following is the Schematic in DOT Graph
        /// <![CDATA[ 
        /// digraph {
        ///     rankdir="LR"
        ///     "0" -> "1" [label= "  1  "];
        ///     "1" -> "0" [label="  -1  "];
        ///     "1" -> "2" [label= "  1  "];
        ///     "2" -> "1" [label="  -1  "];
        ///     "2" -> "3" [label= "  1  "];
        ///     "3" -> "2" [label="  -1  "];
        /// } 
        /// ]]>
        /// <image url="$(SolutionDir)\src\REstate.Concurrency.Primitives\diagram_white.png" />
        /// </remarks>
        private static Schematic<int, int> CreateSemaphoreSchematic(IAgent restateAgent, int slots, int? retryLimit)
        {
            if (slots < 1) throw new ArgumentOutOfRangeException(nameof(slots), "Sempahores must have at least one slot.");

            var mutexBuilder = restateAgent
                .CreateSchematic<int, int>("REstateMutex");

            if (retryLimit == null)
            {
                mutexBuilder.WithStateConflictRetries();
            }
            else if (retryLimit.Value > 0)
            {
                mutexBuilder.WithStateConflictRetries(retryLimit.Value);
            }
            else
            {
                // No retries, so just don't set it.
            }

            mutexBuilder
                .WithState(0, state => state
                    .AsInitialState()
                    .WithReentrance(-1))
                .WithState(1, state => state
                    .WithTransitionFrom(0, 1)
                    .WithTransitionTo(0, -1));

            if (slots == 1)
                return mutexBuilder.Build();

            var sempahoreBuilder = mutexBuilder;

            foreach (var slot in Enumerable.Range(start: 2, count: slots - 1))
            {
                sempahoreBuilder.WithState(slot, state => state
                    .WithTransitionFrom(slot - 1, 1)
                    .WithTransitionTo(slot - 1, -1));
            }

            var semaphore = sempahoreBuilder.Build();

            semaphore.SchematicName = $"REstateSemaphoreOf{slots}";

            return semaphore;
        }
    }
}
