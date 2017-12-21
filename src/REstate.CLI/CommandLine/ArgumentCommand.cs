// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace REstate.CLI.CommandLine
{
    public abstract class ArgumentCommand
    {
        internal ArgumentCommand(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public string Help { get; set; }

        public object Value
        {
            get { return GetValue(); }
        }

        public bool IsHidden { get; set; }

        public bool IsActive { get; private set; }

        internal abstract object GetValue();

        internal void MarkActive()
        {
            IsActive = true;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class ArgumentCommand<T> : ArgumentCommand
    {
        internal ArgumentCommand(string name, T value)
            : base(name)
        {
            Value = value;
        }

        public new T Value { get; private set; }

        internal override object GetValue()
        {
            return Value;
        }
    }
}