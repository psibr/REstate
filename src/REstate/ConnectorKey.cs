using System;
using System.Collections.Generic;

namespace REstate
{
    public struct ConnectorKey 
        : IEquatable<ConnectorKey>
    {
        public ConnectorKey(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ConnectorKey key 
                && Equals(key);
        }

        public bool Equals(ConnectorKey other)
        {
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = 225038131;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public static bool operator ==(ConnectorKey key1, ConnectorKey key2)
        {
            return key1.Equals(key2);
        }

        public static bool operator !=(ConnectorKey key1, ConnectorKey key2)
        {
            return !(key1 == key2);
        }
    }

    public class ConnectorKeyNameEqualityComparer
        : IEqualityComparer<ConnectorKey>
    {
        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        /// <param name="x">The first object of type <see cref="ConnectorKey"/> to compare.</param>
        /// <param name="y">The second object of type <see cref="ConnectorKey"/> to compare.</param>
        public bool Equals(ConnectorKey x, ConnectorKey y)
        {
            return x.Name == y.Name;
        }

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <returns>A hash code for the specified object.</returns>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.</exception>
        public int GetHashCode(ConnectorKey obj)
        {
            return EqualityComparer<string>.Default.GetHashCode(obj.Name);
        }
    }
}