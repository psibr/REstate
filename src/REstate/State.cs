using System;

namespace REstate
{
    public struct State<T>
        : IEquatable<State<T>>
    {
        public State(T value, Guid commitTag)
        {
            Value = value;
            CommitTag = commitTag;
        }

        public State(T value)
            : this(value, Guid.Empty) { }


        public T Value { get; }

        /// <summary>
        /// A value that indicates a unique interaction of state within a machine.
        /// <para />
        /// Empty Guid represents the absence of a commit tag and should not be used.
        /// </summary>
        public Guid CommitTag { get; }

        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(State<T> other)
        {
            return Value.Equals(other.Value);
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
            if (obj is State<T> state)
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
            return Value.GetHashCode();
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
        public static bool IsSameCommit(State<T> a, State<T> b)
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
        public bool IsSameCommit(State<T> other)
        {
            return IsSameCommit(this, other);
        }

        public static bool operator ==(State<T> a, State<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(State<T> a, State<T> b)
        {
            return !(a == b);
        }
    }
}
