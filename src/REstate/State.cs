using System;

namespace REstate
{
    public struct State
        : IEquatable<State>
    {
        public State(string stateName, Guid commitTag)
        {
            if (string.IsNullOrWhiteSpace(stateName))
                throw new ArgumentException("Argument is null or whitespace", nameof(stateName));

            StateName = stateName;
            CommitTag = commitTag;
        }

        public State(string stateName)
            : this(stateName, Guid.Empty) { }


        public string StateName { get; }

        /// <summary>
        /// A value that indicates a unique interaction of state within a machine.
        /// <para />
        /// Empty Guid represents the absence of a commit tag and should not be used.
        /// </summary>
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
            if (obj is State state)
                return Equals(state);

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

        /// <summary>
        /// Verifies that both states have the same value 
        /// AND verifies the commit tags match and are not empty.
        /// </summary>
        /// <param name="a">The left state.</param>
        /// <param name="b">The right state.</param>
        /// <returns>
        /// True if same commit of the same state value;
        /// false if the values or commit tags do not match 
        /// OR if either commit tag is empty.
        /// </returns>
        public static bool IsSameCommit(State a, State b)
        {
            return a == b 
                && a.CommitTag != Guid.Empty 
                && a.CommitTag == b.CommitTag;
        }

        /// <summary>
        /// Verifies that both states have the same value 
        /// AND verifies the commit tags match and are not empty.
        /// </summary>
        /// <param name="other">The state to compare against.</param>
        /// <returns>
        /// True if same commit of the same state value;
        /// false if the values or commit tags do not match 
        /// OR if either commit tag is empty.
        /// </returns>
        public bool IsSameCommit(State other)
        {
            return IsSameCommit(this, other);
        }

        public static bool operator ==(State a, State b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(State a, State b)
        {
            return !(a == b);
        }
    }
}
