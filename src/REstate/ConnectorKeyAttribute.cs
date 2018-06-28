using System;

namespace REstate
{
    /// <summary>
    /// Applied to a StateDefinition to indicate the ConnectorKey value to use in place
    /// of the default form: "Namespace.Class, Assembly".
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class ConnectorKeyAttribute : Attribute
    {
        public ConnectorKeyAttribute(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; private set; }
    }
}