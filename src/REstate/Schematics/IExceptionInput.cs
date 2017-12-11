using System.Diagnostics.CodeAnalysis;

namespace REstate.Schematics
{
    [SuppressMessage("ReSharper", "TypeParameterCanBeVariant")]
    public interface IExceptionInput<TInput>
    {
        TInput Input { get; }
    }
}