using System;

namespace REstate.Engine
{
    public struct State
        : IEquatable<State>
    {
        public State(string stateName, Guid commitTag)
        {
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("Argument is null or whitespace", nameof(stateName));
            
            if (commitTag == Guid.Empty)
                throw new ArgumentException("Must not be an empty Guid.", nameof(commitTag));

            StateName = stateName;
            CommitTag = commitTag;
        }

        public State(string stateName)
        {
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("Argument is null or whitespace", nameof(stateName));

            StateName = stateName;
            CommitTag = Guid.Empty;
        }

        public string StateName { get; }

        public Guid CommitTag { get; }

        public override string ToString()
        {
            return StateName;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(State other)
        {
            return StateName == other.StateName;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is State)
                return Equals((State)obj);

            return false;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return StateName.GetHashCode();
        }

        public static bool IsSameCommit(State a, State b)
        {
            return a == b && a.CommitTag == b.CommitTag;
        }

        public bool IsSameCommit(State other)
        {
            return IsSameCommit(this, other);
        }

        public bool HasChanged(State newState)
        {
            return !IsSameCommit(newState);
        }

        public static bool operator ==(State a, State b)
        {
            return ReferenceEquals(a, b) || (object)a != null && a.Equals(b);
        }

        public static bool operator !=(State a, State b)
        {
            return !(a == b);
        }
    }
}
