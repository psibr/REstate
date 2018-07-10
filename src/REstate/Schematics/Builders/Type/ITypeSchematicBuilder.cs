namespace REstate.Schematics.Builders
{
    public interface ITypeSchematicBuilder<TInput>
    {

        ICreationContext<TInput> StartsIn<TState>() where TState : IStateDefinition;
    }

    public interface ICreationContext<TInput>
    {
        IForStateContext<TState, TInput> For<TState>() where TState : IStateDefinition;

        Schematic<TypeState, TInput> BuildAs(string schematicName);
    }

    public interface IForStateContext<TState, TInput>
         where TState : IStateDefinition
    {
        IOnContext<TState, TInput> On(TInput input);
    }

    public interface IOnContext<TState, TInput>
         where TState : IStateDefinition
    {
        ICreationContext<TInput> MoveTo<TNewState>() where TNewState : IStateDefinition;

        IWhenContext<TState, TInput> When<TPrecondition>();
    }

    public interface IWhenContext<TState, TInput>
         where TState : IStateDefinition
    {
        ICreationContext<TInput> MoveTo<TNewState>() where TNewState : IStateDefinition;
    }
}
