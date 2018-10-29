using System;
using System.Collections.Generic;
using REstate.Schematics;

namespace REstate.Natural
{
    public interface INaturalSchematic : ISchematic<TypeState, TypeState>
    {
    }

    public class NaturalSchematic : INaturalSchematic
    {
        private readonly Schematic<TypeState, TypeState> _schematic;

        public NaturalSchematic(Schematic<TypeState, TypeState> schematic)
        {
            _schematic = schematic;
        }

        public NaturalSchematic(ISchematic<TypeState, TypeState> schematic)
        {
            _schematic = schematic as Schematic<TypeState, TypeState> ?? schematic.Clone();
        }

        public string SchematicName => _schematic.SchematicName;

        public TypeState InitialState => _schematic.InitialState;

        public int StateConflictRetryCount
        {
            get => _schematic.StateConflictRetryCount;
            set
            {
                if(value < -1) 
                    throw new ArgumentException("Values below -1 are not valid.", nameof(value));

                _schematic.StateConflictRetryCount = value;
            }
        }

        public IReadOnlyDictionary<TypeState, IState<TypeState, TypeState>> States => 
            (_schematic as ISchematic<TypeState, TypeState>).States;

        public override string ToString()
        {
            return NaturalDotGraphCartographer.Instance.WriteMap(this);
        }
    }
}
