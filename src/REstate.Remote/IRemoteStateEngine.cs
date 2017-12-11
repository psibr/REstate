using REstate.Engine;

namespace REstate.Remote
{
    public interface IRemoteStateEngine<TState, TInput>
        : IStateEngine<TState, TInput>
    {
    }
}