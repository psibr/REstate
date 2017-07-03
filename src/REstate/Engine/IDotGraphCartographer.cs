using System.Collections.Generic;
using REstate.Configuration;

namespace REstate.Engine
{
    public interface ICartographer<TState, TInput>
    {
        string WriteMap(IDictionary<State<TState>, StateConfiguration<TState, TInput>> configuration);
    }
}