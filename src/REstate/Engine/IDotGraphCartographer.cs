using System.Collections.Generic;
using REstate.Configuration;

namespace REstate.Engine
{
    public interface ICartographer
    {
        string WriteMap(IDictionary<State, StateConfiguration> configuration);
    }
}