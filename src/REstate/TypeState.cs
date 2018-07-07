using System;
using REstate.Engine.Connectors;

namespace REstate
{

    /// <summary>
    /// Represents a Type as used as a State in a Schematic or Machine.
    /// </summary>
    public class TypeState : IEquatable<TypeState>
    {
        private string _connectorKey;
        private string _assemblyQualifiedName;
        private string _fullName;
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

        public string FullName
        {
            get
            {
                // If for some reason full name wasn't set at construction,
                // we know how to get to it anyway.
                if(_fullName == null)
                {
                    return GetFullNameFrom(AssemblyQualifiedName);
                }

                return _fullName;
            }
            set => _fullName = value;
        }

        public string ConnectorKey
        {
            get
            {
                // If no ConnectorKey use FullName.
                if (_connectorKey == null)
                {
                    return FullName;
                }

                return _connectorKey;
            }
            set => _connectorKey = value;
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
            => Type.GetType(FullName);

        private bool CheckIsActionable()
            => typeof(IAction).IsAssignableFrom(_underlyingType.Value);

        private bool CheckIsPrecondition()
            => typeof(IPrecondition).IsAssignableFrom(_underlyingType.Value);

        public bool Equals(TypeState other)
        {
            return FullName.Equals(other.FullName, StringComparison.Ordinal);
        }

        private string GetFullNameFrom(string assemblyQualifiedName)
        {
            return assemblyQualifiedName.Substring(0, assemblyQualifiedName.IndexOf(',', assemblyQualifiedName.IndexOf(',') + 1));
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
            return FullName.GetHashCode();
        }

        public static implicit operator TypeState(Type type)
            => FromType(type);

        public static implicit operator Type(TypeState typeState)
            => typeState.ToType();

        public override string ToString()
        {
            return AssemblyQualifiedName;
        }

        public static TypeState FromType(Type type)
        {
            return new TypeState
            {
                AssemblyQualifiedName = type.AssemblyQualifiedName,
                FullName = $"{type.FullName}, {type.Assembly.GetName().Name}"
            };
        }
    }
}