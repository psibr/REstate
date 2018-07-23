using REstate.Schematics;

namespace REstate
{
    public interface ITypeSchematicFactory
    {
        Schematic<TypeState, TypeState> BuildSchematic(IAgent agent);
    }
}