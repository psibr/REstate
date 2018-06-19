namespace REstate.Schematics.TypedBuilder
{
    public interface ITypedSchematicBuilder<TInput>
    {

        ICreationContext<TInput> StartsIn<TState>();
    }

    public interface ICreationContext<TInput>
    {
        IForStateContext<TState, TInput> For<TState>();

        Schematic<TypeState, TInput> BuildAs(string schematicName);
    }

    public interface IForStateContext<TState, TInput>
    {
        IOnContext<TState, TInput> On(TInput input);
    }

    public interface IOnContext<TState, TInput>
    {
        ICreationContext<TInput> MoveTo<TNewState>();

        IWhenContext<TState, TInput> When<TPreconddition>();
    }

    public interface IWhenContext<TState, TInput>
    {
        ICreationContext<TInput> MoveTo<TNewState>();
    }
}
