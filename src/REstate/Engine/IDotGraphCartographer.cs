using System.Collections.Generic;
using REstate.Configuration;

namespace REstate.Engine
{
    public interface ICartographer<TState, TInput>
    {
        string WriteMap(ICollection<StateConfiguration<TState, TInput>> configuration);
    }
}