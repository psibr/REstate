using System;
using System.Reflection;
using REstate.Engine.Connectors;

namespace REstate.Schematics
{
    /// <summary>
    /// Represents a Type used as a State in a Schematic or Machine.
    /// </summary>
    public class TypeState : IEquatable<TypeState>
    {
        private string _assemblyQualifiedName;
        private Lazy<Type> _underlyingType;
        private Lazy<bool> _isActionable;
        private Lazy<bool> _isPrecondition;

        public string AssemblyQualifiedName
        {
            get => _assemblyQualifiedName;
            set
            {
                _assemblyQualifiedName = value;

                _underlyingType = new Lazy<Type>(GetUnderlyingType);
                _isActionable = new Lazy<bool>(CheckIsActionable);
                _isPrecondition = new Lazy<bool>(CheckIsPrecondition);
            }
        }

        public bool AcceptsInputOf<TInput>() => 
            (!IsActionable()
                || typeof(IAction<TypeState, TInput>).IsAssignableFrom(_underlyingType.Value))
            && (!IsPrecondition()
                || typeof(IPrecondition).IsAssignableFrom(_underlyingType.Value));

        public bool IsActionable() => _isActionable.Value;

        public bool IsPrecondition() => _isPrecondition.Value;

        public Type ToType() => _underlyingType.Value;

        private Type GetUnderlyingType()
            => Type.GetType(_assemblyQualifiedName);

        private bool CheckIsActionable()
            => typeof(IAction).IsAssignableFrom(_underlyingType.Value);

        private bool CheckIsPrecondition()
            => typeof(IPrecondition).IsAssignableFrom(_underlyingType.Value);

        public bool Equals(TypeState other)
        {
            return this._assemblyQualifiedName == other._assemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (obj is TypeState typeState)
            {
                return Equals(typeState);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this._assemblyQualifiedName.GetHashCode();
        }

        public static implicit operator TypeState(Type type)
            => new TypeState { AssemblyQualifiedName = type.AssemblyQualifiedName };

        public static implicit operator Type(TypeState typeState)
            => typeState.ToType();
    }
}