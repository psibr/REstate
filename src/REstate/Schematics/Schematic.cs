using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using REstate.Engine;

namespace REstate.Schematics
{
    /// <summary>
    /// Mutable object format for Schematics, a definition of a state machine.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TInput"></typeparam>
    public class Schematic<TState, TInput> 
        : ISchematic<TState, TInput>
    {
        public Schematic(string schematicName, TState initialState, int stateConflictRetryCount, params State<TState, TInput>[] states)
        {
            SchematicName = schematicName;
            InitialState = initialState;
            StateConflictRetryCount = stateConflictRetryCount;
            States = states;
        }

        public Schematic()
        {
        }

        [Required]
        public string SchematicName { get; set; }

        /// <summary>
        /// The initial state of the machine when created.
        /// </summary>
        [Required]
        public TState InitialState { get; set; }

        /// <summary>
        /// Allows the machine to retry an update when a <see cref="StateConflictException"/> occurs.
        /// </summary>
        public int StateConflictRetryCount { get; set; }

        /// <inheritdoc />
        IReadOnlyDictionary<TState, IState<TState, TInput>> ISchematic<TState, TInput>.States =>
            States.ToDictionary(
                c => c.Value,
                c => (IState<TState, TInput>)c);

        /// <summary>
        /// States that are available to the machine.
        /// 
        /// <para>
        ///     States may also declare transitions, actions, preconditions, and substates.
        /// </para>
        /// </summary>
        /// <remarks>Used for serialization and interchange primarily. See <see cref="ISchematic&lt;TState, TInput&gt;.States"/> for the readonly view.</remarks>
        [Required]
        public State<TState, TInput>[] States { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => 
            DotGraphCartographer<TState, TInput>.Instance.WriteMap(this);
    }
}
