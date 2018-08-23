namespace REstate.Natural.Schematics.Builders
{
    /// <summary>
    /// Builder for natural Schematics.
    /// </summary>
    public interface INaturalSchematicBuilder
    { 
        ICreationContext StartsIn<TState>() where TState : IStateDefinition;
    }

    /// <summary>
    /// The main context of a <see cref="INaturalSchematicBuilder"/>.
    /// </summary>
    public interface ICreationContext
    {
        /// <summary>
        /// Moves the context to a specific state.
        /// </summary>
        /// <typeparam name="TState">The State</typeparam>
        /// <returns>A context specific to <typeparamref name="TState"/></returns>
        IForStateContext<TState> For<TState>() where TState : IStateDefinition;

        /// <summary>
        /// Completes the building process and names the Schematic.
        /// </summary>
        /// <param name="schematicName">The name of the Schematic</param>
        /// <returns>A natural Schematic</returns>
        INaturalSchematic BuildAs(string schematicName);
    }

    /// <summary>
    /// A context specific to a State
    /// </summary>
    /// <typeparam name="TState">The State</typeparam>
    public interface IForStateContext<TState>
         where TState : IStateDefinition
    {
        /// <summary>
        /// Begins defining a transition for the current State in context and a Signal
        /// </summary>
        /// <typeparam name="TSignal">The Signal to allow</typeparam>
        /// <returns>A context specific to <typeparamref name="TState"/> and <typeparamref name="TSignal"/></returns>
        IOnContext<TState, TSignal> On<TSignal>();
    }

    /// <summary>
    /// A context of a transition from a State using a Signal
    /// </summary>
    /// <typeparam name="TState">The State to transition from</typeparam>
    /// <typeparam name="TSignal">The Signal to allow</typeparam>
    public interface IOnContext<TState, out TSignal>
         where TState : IStateDefinition
    {
        /// <summary>
        /// Finishes defining a transition by providing the destination State
        /// </summary>
        /// <typeparam name="TNewState">The State to transition to</typeparam>
        /// <returns></returns>
        ICreationContext MoveTo<TNewState>() where TNewState : IStateDefinition<TSignal>;

        IWhenContext<TState, TSignal> When<TPrecondition>() where TPrecondition : INaturalPrecondition<TSignal>;
    }

    public interface IWhenContext<TState, out TSignal>
         where TState : IStateDefinition
    {
        ICreationContext MoveTo<TNewState>() where TNewState : IStateDefinition<TSignal>;
    }
}
