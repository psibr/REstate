using REstate.IoC;
using REstate.IoC.BoDi;
using Xunit;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext
    {
        public REstateHost CurrentHost { get; set; }

        public void Given_a_new_host()
        {
            CurrentHost = new REstateHost();
        }

        public void Given_a_new_host_with_custom_ComponentContainer(IComponentContainer componentContainer)
        {
            CurrentHost = new REstateHost(componentContainer);
        }
    }
}