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
        IOnContext<TState, TRequest> On<TRequest>();
    }

    public interface IOnContext<TState, TRequest>
         where TState : IStateDefinition
    {
        ICreationContext MoveTo<TNewState>() where TNewState : IStateDefinition;

        IWhenContext<TState> When<TPrecondition>();
    }

    public interface IWhenContext<TState>
         where TState : IStateDefinition
    {
        ICreationContext MoveTo<TNewState>() where TNewState : IStateDefinition;
    }
}
