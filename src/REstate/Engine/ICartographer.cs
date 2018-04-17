using REstate.Schematics;

namespace REstate.Engine
{
    public interface ICartographer<TState, TInput>
    {
        string WriteMap(ISchematic<TState, TInput> schematic);
    }
}
