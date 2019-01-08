using System;

namespace REstate.Natural
{
    /// <summary>
    /// Applied to an Action to indicates signalers should not wait for subsequent state changes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LongRunningAttribute : Attribute
    {
        public LongRunningAttribute()
        {
        }
    }
}