using System;

namespace REstate
{
    /// <summary>
    /// Applied to a StateDefinition to indicate the State's name instead of class name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class StateNameAttribute : Attribute
    {
        public StateNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}