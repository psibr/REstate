using System;
using System.Threading;
using System.Threading.Tasks;
using REstate.Schematics;

namespace REstate.Engine.Repositories
{
    public interface ISchematicRepository<TState, TInput>
    {
        /// <summary>
        /// Retrieves a previously stored Schematic by name.
        /// </summary>
        /// <param name="schematicName">The name of the Schematic</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schematicName"/> is null.</exception>
        /// <exception cref="SchematicDoesNotExistException">Thrown when no matching Schematic was found for the given name.</exception>
        Task<Schematic<TState, TInput>> RetrieveSchematicAsync(string schematicName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores a Schematic, using its <c>SchematicName</c> as the key.
        /// </summary>
        /// <param name="schematic">The Schematic to store</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schematic"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="schematic"/> has a null <c>SchematicName</c> property.</exception>
        Task<Schematic<TState, TInput>> StoreSchematicAsync(Schematic<TState, TInput> schematic, CancellationToken cancellationToken = default);
    }
}
