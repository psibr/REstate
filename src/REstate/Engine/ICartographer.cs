using System.Collections.Generic;
using REstate.Schematics;

namespace REstate.Engine
{
    public interface ICartographer<TState, TInput>
    {
        string WriteMap(IEnumerable<IState<TState, TInput>> states);
    }
}
