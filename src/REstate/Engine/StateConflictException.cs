using System;

namespace REstate.Engine
{
    /// <summary>
    /// Indicates a state change conflicted with another change, no update was made.
    /// </summary>
    public class StateConflictException
        : Exception
    {
        public StateConflictException()
            : this("CommitTag did not match; no update performed.")
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
    }
}
