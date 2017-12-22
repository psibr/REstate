using Xunit;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext
    {
        public IHostConfiguration CurrentHostConfiguration { get; set; }

        public void When_configuration_is_accessed()
        {
            CurrentHostConfiguration = CurrentHost.Agent().Configuration;
        }

        public void Then_configuration_has_a_container()
        {
            Assert.NotNull(((HostConfiguration)CurrentHostConfiguration).Container);
        }

        public void Then_configuration_is_not_null()
        {
            Assert.NotNull(CurrentHostConfiguration);
        }
    }
}