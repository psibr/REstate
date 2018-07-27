using System.Collections.Generic;
using REstate.Schematics;

namespace REstate.Natural
{
    public interface INaturalSchematic : ISchematic<TypeState, TypeState>
    {
    }

    public class NaturalSchematic : INaturalSchematic
    {
        private readonly ISchematic<TypeState, TypeState> _schematic;

        public NaturalSchematic(ISchematic<TypeState, TypeState> schematic)
        {
            _schematic = schematic;
        }

        public string SchematicName => _schematic.SchematicName;

        public TypeState InitialState => _schematic.InitialState;

        public int StateConflictRetryCount => _schematic.StateConflictRetryCount;

        public IReadOnlyDictionary<TypeState, IState<TypeState, TypeState>> States => _schematic.States;
    }
}
