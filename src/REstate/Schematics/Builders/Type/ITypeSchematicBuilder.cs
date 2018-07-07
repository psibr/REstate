namespace REstate.Schematics.Builders
{
    public interface ITypeSchematicBuilder<TInput>
    {

        ICreationContext<TInput> StartsIn<TState>() where TState : IStateDefinition, new();
    }

    public interface ICreationContext<TInput>
    {
        IForStateContext<TState, TInput> For<TState>() where TState : IStateDefinition, new();

        Schematic<TypeState, TInput> BuildAs(string schematicName);
    }

    public interface IForStateContext<TState, TInput>
         where TState : IStateDefinition, new()
    {
        IOnContext<TState, TInput> On(TInput input);
    }

    public interface IOnContext<TState, TInput>
         where TState : IStateDefinition, new()
    {
        ICreationContext<TInput> MoveTo<TNewState>() where TNewState : IStateDefinition, new();

        IWhenContext<TState, TInput> When<TPrecondition>();
    }

    public interface IWhenContext<TState, TInput>
         where TState : IStateDefinition, new()
    {
        ICreationContext<TInput> MoveTo<TNewState>() where TNewState : IStateDefinition, new();
    }
}
