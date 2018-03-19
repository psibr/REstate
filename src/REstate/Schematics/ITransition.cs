// ReSharper disable TypeParameterCanBeVariant
namespace REstate.Schematics
{
    public interface ITransition<TState, TInput>
    {
        TInput Input { get; }

        TState ResultantState { get; }

        IPrecondition Procondition { get; }
    }
}