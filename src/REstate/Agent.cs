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
    }
}
