using System;
using System.Threading.Tasks;
using Xunit;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext
    {
        public IHostConfiguration CurrentHostConfiguration { get; set; }

        #region GIVEN

        #endregion

        #region WHEN
        public Task When_configuration_is_accessed()
        {
            try
            {
                CurrentHostConfiguration = CurrentHost.Agent().Configuration;
            }
            catch (Exception ex)
            {
                CurrentException = ex;
            }

            return Task.CompletedTask;
        }
        #endregion

        #region THEN
        public Task Then_configuration_has_a_container()
        {
            Assert.NotNull(((HostConfiguration)CurrentHostConfiguration).Container);

            return Task.CompletedTask;
        }

        public Task Then_configuration_is_not_null()
        {
            Assert.NotNull(CurrentHostConfiguration);

            return Task.CompletedTask;
        }
        #endregion

    }
}