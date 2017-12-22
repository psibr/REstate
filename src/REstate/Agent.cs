using REstate.Engine;
using REstate.Schematics;

namespace REstate
{
    public interface IAgent
    {
        IHostConfiguration Configuration { get; }
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
                .WriteMap(schematic.States);
    }
}
