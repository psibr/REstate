using System;
using System.Collections.Generic;
using REstate.Configuration;
using REstate.IoC;

namespace REstate.Engine
{
    public interface ICartographer<TState>
    {
        string WriteMap(IDictionary<State<TState>, StateConfiguration<TState>> configuration);
    }
}