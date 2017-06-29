using System;

namespace REstate
{
    public struct Input
        : IEquatable<Input>
    {
        public Input(string inputName)
        {
            InputName = inputName;
        }

        public string InputName { get; }

        public override string ToString()
        {
            return InputName;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Input other)
        {
            return InputName.Equals(other.InputName);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj is Input input)
                return Equals(input);

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
            return InputName.GetHashCode();
        }

        public static bool operator ==(Input a, Input b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Input a, Input b)
        {
            return !(a == b);
        }

        public static implicit operator Input(string inputName)
        {
            return new Input(inputName);
        }

        public static implicit operator string(Input input)
        {
            return input.InputName;
        }
    }
}
