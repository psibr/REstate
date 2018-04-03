using System;
using System.Collections.Generic;

namespace REstate.Schematics
{
    public class ConnectorKey
        : IEquatable<ConnectorKey>
    {
        private string _identifier;

        public string Identifier
        {
            get => _identifier;
            set
            {
                if (value is null)
                    throw new ArgumentNullException(
                        paramName: nameof(value),
                        message: $"{nameof(ConnectorKey)} cannot have a null {nameof(Identifier)}.");

                if(string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException(
                        message: $"{nameof(ConnectorKey)} must have a valid non-whitespace {nameof(Identifier)}.",
                        paramName: nameof(value));

                _identifier = value;
            }
        }

        public override string ToString() => Identifier;

        public override bool Equals(object obj)
        {
            return Equals(obj as ConnectorKey);
        }

        public bool Equals(ConnectorKey other)
        {
            return other != null 
                   && Identifier == other.Identifier;
        }

        public override int GetHashCode()
        {
            return 1186239758 + EqualityComparer<string>.Default.GetHashCode(Identifier);
        }

        public static bool operator ==(ConnectorKey key1, ConnectorKey key2)
        {
            return EqualityComparer<ConnectorKey>.Default.Equals(key1, key2);
        }

        public static bool operator !=(ConnectorKey key1, ConnectorKey key2)
        {
            return !(key1 == key2);
        }

        public static implicit operator ConnectorKey(string identifier)
        {
            return new ConnectorKey { Identifier = identifier };
        }
    }
}