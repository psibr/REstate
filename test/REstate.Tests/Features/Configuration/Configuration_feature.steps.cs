using REstate.IoC.BoDi;
using Xunit;
using Xunit.Abstractions;

namespace REstate.Tests.Features.Configuration
{
    public partial class Configuration_feature
    {
        private void Given_the_default_container_is_used()
        {
            
        }

        private void Given_a_custom_container_is_used()
        {
            REstateHost.UseContainer(
                container: new BoDiComponentContainer(
                    container: new ObjectContainer()));
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
        public Configuration_feature(ITestOutputHelper output) 
            : base(output)
        {
        }
        #endregion
    }
}