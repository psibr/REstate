namespace REstate.Natural.Schematics.Builders
{
    public interface INaturalSchematicBuilder
    {

        ICreationContext StartsIn<TState>() where TState : IStateDefinition;
    }

    public interface ICreationContext
    {
        IForStateContext<TState> For<TState>() where TState : IStateDefinition;

        INaturalSchematic BuildAs(string schematicName);
    }

    public interface IForStateContext<TState>
         where TState : IStateDefinition
    {
        IOnContext<TState, TSignal> On<TSignal>();
    }

    public interface IOnContext<TState, out TSignal>
         where TState : IStateDefinition
    {
        //ICreationContext MoveTo<TNewState>() where TNewState : IStateDefinition;

        ICreationContext MoveTo<TNewState>() where TNewState : IStateDefinition<TSignal>;

        IWhenContext<TState, TSignal> When<TPrecondition>();
    }

    public interface IWhenContext<TState, out TSignal>
         where TState : IStateDefinition
    {
        ICreationContext MoveTo<TNewState>() where TNewState : IStateDefinition<TSignal>;
    }
}
