using System.Collections.Generic;

namespace REstate.Schematics
{
    /// <summary>
    /// Definition of a state machine.
    /// </summary>
    /// <typeparam name="TState">The type of the states in the machine. All states must have a common base type.</typeparam>
    /// <typeparam name="TInput">The type of the input or signals that can be sent to the machine. All input must have a common base type.</typeparam>
    public interface ISchematic<TState, TInput>
    {
        string SchematicName { get; }
        TState InitialState { get; }
        int StateConflictRetryCount { get; }

        /// <summary>
        /// States that are available to the machine.
        /// 
        /// <para>
        ///     States may also declare transitions, actions, preconditions, and substates.
        /// </para>
        /// </summary>
        IReadOnlyDictionary<TState, IState<TState, TInput>> States { get; }
    }
}