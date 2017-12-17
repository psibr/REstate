using REstate.IoC.BoDi;
using Xunit;
using Xunit.Abstractions;

namespace REstate.Tests.Features.Configuration
{
    public partial class Configuration
    {
        private void Given_a_new_host()
        {
            REstateHost.ResetAgent();
        }

        private void Given_a_new_container_is_configured()
        {
            REstateHost.UseContainer(
                container: new BoDiComponentContainer(new ObjectContainer()));
        }

        private void When_configuration_is_accessed()
        {
            var configuration = REstateHost.Agent.Configuration;
        }

        private void Then_configuration_has_a_container()
        {
            Assert.NotNull(((HostConfiguration)REstateHost.Agent.Configuration).Container);
        }

        private void Then_configuration_is_not_null()
        {
            Assert.NotNull(REstateHost.Agent.Configuration);
        }

        #region Constructor
        public Configuration(ITestOutputHelper output) 
            : base(output)
        {
        }
        #endregion
    }
}