using System;

namespace REstate.Natural
{
    /// <summary>
    /// Applied to a StateDefinition, Action, or Precondition to add a description.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }
}