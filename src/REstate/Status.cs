using System;
using System.Collections.Generic;

namespace REstate
{
    public struct Status<T>
        : IEquatable<Status<T>>
    {
        public Status(string machineId, T state, Guid commitTag)
        {
            MachineId = machineId;
            State = state;
            CommitTag = commitTag;
        }

        public Status(string machineId, T state)
            : this(machineId, state, Guid.Empty) { }


        public T State { get; }

        /// <summary>
        /// A value that indicates a unique interaction of state within a machine.
        /// <para />
        /// Empty Guid represents the absence of a commit tag and should not be used.
        /// </summary>
        public Guid CommitTag { get; }

        public string MachineId { get; }

        public override string ToString()
        {
            return State.ToString();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Status<T> other)
        {
            return MachineId.Equals(other.MachineId) && State.Equals(other.State);
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
            if (obj is Status<T> state)
                return Equals(state);

            return false;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<T>.Default.GetHashCode(State);
                hashCode = (hashCode * 397) ^ MachineId.GetHashCode();
                return hashCode;
            }
        }


        /// <summary>
        /// Verifies that both statuses have the same state
        /// AND verifies the commit tags match and are not empty.
        /// </summary>
        /// <param name="a">The left state.</param>
        /// <param name="b">The right state.</param>
        /// <returns>
        /// True if same commit of the same state;
        /// false if the values or commit tags do not match 
        /// OR if either commit tag is empty.
        /// </returns>
        public static bool IsSameCommit(Status<T> a, Status<T> b)
        {
            return a == b 
                && a.CommitTag != Guid.Empty 
                && a.CommitTag == b.CommitTag;
        }

        /// <summary>
        /// Verifies that both statuses have the same state 
        /// AND verifies the commit tags match and are not empty.
        /// </summary>
        /// <param name="other">The state to compare against.</param>
        /// <returns>
        /// True if same commit of the same state value;
        /// false if the values or commit tags do not match 
        /// OR if either commit tag is empty.
        /// </returns>
        public bool IsSameCommit(Status<T> other)
        {
            return IsSameCommit(this, other);
        }

        public static bool operator ==(Status<T> a, Status<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Status<T> a, Status<T> b)
        {
            return !(a == b);
        }
    }
}
