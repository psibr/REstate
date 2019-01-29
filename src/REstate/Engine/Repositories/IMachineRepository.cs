using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using REstate.Schematics;

namespace REstate.Engine.Repositories
{
    public interface IMachineRepository<TState, TInput>
    {
        /// <summary>
        /// Creates a new Machine from a provided Schematic.
        /// </summary>
        /// <param name="schematicName">The name of the stored Schematic</param>
        /// <param name="machineId">The Id of Machine to create; if null, an Id will be generated.</param>
        /// <param name="metadata">Related metadata for the Machine</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schematicName"/> is null.</exception>
        Task<MachineStatus<TState, TInput>> CreateMachineAsync(string schematicName, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new Machine from a provided Schematic.
        /// </summary>
        /// <param name="schematic">The Schematic of the Machine</param>
        /// <param name="machineId">The Id of Machine to create; if null, an Id will be generated.</param>
        /// <param name="metadata">Related metadata for the Machine</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="schematic"/> is null.</exception>
        /// <exception cref="SchematicDoesNotExistException">Thrown when no matching Schematic was found for the given name.</exception>
        Task<MachineStatus<TState, TInput>> CreateMachineAsync(Schematic<TState, TInput> schematic, string machineId, IDictionary<string, string> metadata, CancellationToken cancellationToken = default);

        Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            Schematic<TState, TInput> schematic,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        Task<ICollection<MachineStatus<TState, TInput>>> BulkCreateMachinesAsync(
            string schematicName,
            IEnumerable<IDictionary<string, string>> metadata,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a Machine.
        /// </summary>
        /// <remarks>
        /// Does not throw an exception if a matching Machine was not found.
        /// </remarks>
        /// <param name="machineId">The Id of the Machine to delete</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="machineId"/> is null.</exception>
        Task DeleteMachineAsync(string machineId, CancellationToken cancellationToken = default);
    }
}
