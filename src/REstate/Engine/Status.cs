using System;
using System.Collections.Generic;

namespace REstate.Engine
{
    /// <summary>
    /// An immutable structure that represents some interaction of state in a <see cref="T:REstate.IStateMachine`2" />
    /// </summary>
    public struct Status<T>
        : IEquatable<Status<T>>
    {
        /// <summary>
        /// Constructs an immutable status record
        /// </summary>
        /// <param name="machineId">The unique identifier of the machine to which this status relates</param>
        /// <param name="state">The state to be represented by the status</param>
        /// <param name="updatedTime">The date and time the state interaction occured; the value will be converted to UTC</param>
        /// <param name="commitTag">A unique identifier for this state interaction</param>
        public Status(string machineId, T state, DateTimeOffset updatedTime, Guid commitTag)
        {
            MachineId = machineId;
            State = state;
            UpdatedTime = updatedTime;
            CommitTag = commitTag;
        }

        /// <summary>
        /// The state to be represented
        /// </summary>
        public T State { get; }

        /// <summary>
        /// A value that indicates a unique interaction of state within a machine.
        /// <para />
        /// Empty Guid represents the absence of a commit tag and should not be used.
        /// </summary>
        public Guid CommitTag { get; }

        /// <summary>
        /// A unique identifier for the machine to which the status is related
        /// </summary>
        public string MachineId { get; }

        /// <summary>
        /// The date and time this status change occured
        /// </summary>
        /// <returns>A UTC offset date and time</returns>
        public DateTimeOffset UpdatedTime { get; }

        /// <summary>
        /// Presents the state the status represents in a textual form
        /// </summary>
        /// <returns>The state as a <see cref="string" /></returns>
        public override string ToString()
        {
            return State.ToString();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref identifier="other"/> parameter; otherwise, false.
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

        public static bool operator ==(Status<T> a, Status<T> b) => a.Equals(b);

        public static bool operator !=(Status<T> a, Status<T> b) => !(a == b);
    }
}
