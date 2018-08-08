using REstate.Engine;
using REstate.Engine.Connectors.Resolution;
using REstate.Schematics;

namespace REstate
{
    public interface IAgent
    {
        IHostConfiguration Configuration { get; }

        TStateEngine GetStateEngine<TState, TInput, TStateEngine>()
            where TStateEngine : class, IStateEngine<TState, TInput>;

        IStateEngine<TState, TInput> GetStateEngine<TState, TInput>();
    }

    internal class Agent 
        : IAgent
    {
        public IHostConfiguration Configuration { get; }

        public Agent(HostConfiguration hostConfiguration)
        {
            Configuration = hostConfiguration;
        }

        public string GetStateMap<TState, TInput>(Schematic<TState, TInput> schematic) =>
            ((HostConfiguration)Configuration).Container
                .Resolve<ICartographer<TState, TInput>>()
                .WriteMap(schematic);

        public IStateEngine<TState, TInput> GetStateEngine<TState, TInput>() =>
            ((HostConfiguration)Configuration).Container
                .Resolve<IStateEngine<TState, TInput>>();

        public TStateEngine GetStateEngine<TState, TInput, TStateEngine>()
            where TStateEngine : class, IStateEngine<TState, TInput> =>
            ((HostConfiguration)Configuration).Container
                .Resolve<TStateEngine>();
    }
}
