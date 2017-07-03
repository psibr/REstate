//using System;

//namespace REstate
//{
//    public struct TInput
//        : IEquatable<TInput>
//    {
//        public Input(TInput input)
//        {
//            Value = input;
//        }

//        public TInput Value { get; }

//        public override string ToString()
//        {
//            return Value.ToString();
//        }

//        /// <summary>
//        /// Indicates whether the current object is equal to another object of the same type.
//        /// </summary>
//        /// <returns>
//        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
//        /// </returns>
//        /// <param name="other">An object to compare with this object.</param>
//        public bool Equals(TInput other)
//        {
//            return Value.Equals(other.Value);
//        }

//        /// <summary>
//        /// Determines whether the specified object is equal to the current object.
//        /// </summary>
//        /// <returns>
//        /// true if the specified object is equal to the current object; otherwise, false.
//        /// </returns>
//        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
//        public override bool Equals(object obj)
//        {
//            if (obj is TInput input)
//                return Equals(input);

//            return false;
//        }

//        /// <summary>
//        /// Serves as the default hash function.
//        /// </summary>
//        /// <returns>
//        /// A hash code for the current object.
//        /// </returns>
//        public override int GetHashCode()
//        {
//            return Value.GetHashCode();
//        }

//        public static bool operator ==(TInput a, TInput b)
//        {
//            return a.Equals(b);
//        }

//        public static bool operator !=(TInput a, TInput b)
//        {
//            return !(a == b);
//        }

//        public static implicit operator TInput(TInput input)
//        {
//            return new TInput(input);
//        }

//        public static implicit operator TInput(TInput input)
//        {
//            return input.Value;
//        }
//    }
//}
