using System;
using System.Reflection;
using REstate.Engine.Connectors;
using REstate.Natural;

namespace REstate
{
    /// <summary>
    /// Represents a Type as used as a State in a Schematic or Machine.
    /// </summary>
    public class TypeState : IEquatable<TypeState>
    {
        private string _name;
        private string _stateName;
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

        public string GetFullName()
        {
            return _fullName ?? (_fullName = GetFullNameFrom(AssemblyQualifiedName));
        }

        public string GetName()
        {
            return _name ?? (_name = GetNameFrom(AssemblyQualifiedName));
        }

        public string GetStateName()
        {
            if (_stateName == null)
            {
                // Check for a StateNameAttribute on the type
                var stateNameAttribute = _underlyingType
                    .Value.GetCustomAttribute<StateNameAttribute>();

                if (!string.IsNullOrWhiteSpace(stateNameAttribute?.Name))
                    _stateName = stateNameAttribute.Name;
                else // If no StateName use name.
                    _stateName = GetName();
            }

            return _stateName;
        }

        public string GetConnectorKey() => GetFullName();

        public bool IsActionable() => _isActionable.Value;

        public bool IsPrecondition() => _isPrecondition.Value;

        public Type ToType() => _underlyingType.Value;

        private Type GetUnderlyingType()
            => Type.GetType(GetFullName());

        private bool CheckIsActionable()
            => typeof(IAction).IsAssignableFrom(_underlyingType.Value);

        private bool CheckIsPrecondition()
            => typeof(IPrecondition).IsAssignableFrom(_underlyingType.Value);

        private static string GetFullNameFrom(string assemblyQualifiedName)
        {
            return assemblyQualifiedName.Substring(0, assemblyQualifiedName.IndexOf(',', assemblyQualifiedName.IndexOf(',') + 1));
        }

        private static string GetNameFrom(string assemblyQualifiedName)
        {
            var nameEndIndex = assemblyQualifiedName.IndexOf(',');

            var noAssemblyName = assemblyQualifiedName.Substring(0, nameEndIndex);

            var lastPeriodIndex = noAssemblyName.LastIndexOf('.');

            var name = noAssemblyName.Substring(lastPeriodIndex + 1, noAssemblyName.Length - lastPeriodIndex - 1);

            return name;
        }

        public bool Equals(TypeState other)
        {
            return other != null && GetFullName().Equals(other.GetFullName(), StringComparison.Ordinal);
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
            return GetFullName().GetHashCode();
        }

        public static implicit operator TypeState(Type type)
            => FromType(type);

        public static implicit operator Type(TypeState typeState)
            => typeState.ToType();

        public override string ToString()
        {
            return GetStateName();
        }

        public static TypeState FromType(Type type)
        {
            return new TypeState
            {
                AssemblyQualifiedName = type.AssemblyQualifiedName,
                _name = type.Name
            };
        }
    }
}