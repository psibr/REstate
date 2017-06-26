using System;

namespace REstate.Engine
{
    public struct Trigger
        : IEquatable<Trigger>
    {
        public Trigger(string triggerName)
        {
            if (string.IsNullOrWhiteSpace(triggerName))
                throw new ArgumentException("Cannot be null or whitespace", nameof(triggerName));

            TriggerName = triggerName;
        }

        public string TriggerName { get; }

        public override string ToString()
        {
            return TriggerName;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Trigger other)
        {
            return other != null
                   && TriggerName == other.TriggerName;
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

            if (obj is Trigger)
                return Equals((Trigger)obj);

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
            return TriggerName.GetHashCode();
        }

        public static bool operator ==(Trigger a, Trigger b)
        {
            return ReferenceEquals(a, b) || (object)a != null && a.Equals(b);
        }

        public static bool operator !=(Trigger a, Trigger b)
        {
            return !(a == b);
        }

        public static implicit operator Trigger(string triggerName)
        {
            return new Trigger(triggerName);
        }

        public static implicit operator String(Trigger trigger)
        {
            return trigger.TriggerName;
        }
    }
}
