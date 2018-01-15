using System;

namespace REstate.Engine
{
    /// <summary>
    /// Indicates a state change conflicted with another change, no update was made.
    /// </summary>
    public class StateConflictException
        : Exception
    {
        private const string StandardMessage = "CommitTag did not match; no update performed.";

        public StateConflictException()
            : this(StandardMessage)
        {
        }

        public StateConflictException(string message) 
            : base(message)
        {
        }

        public StateConflictException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public StateConflictException(Exception innerException)
            : this(StandardMessage, innerException)
        {

        }
    }
}
