using System;
using System.Threading.Tasks;
using REstate.IoC;
using Xunit;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext
    {
        public REstateHost CurrentHost { get; set; }

        public Exception CurrentException { get; set; }

        public Task Given_a_new_host()
        {
            CurrentHost = new REstateHost();

            return Task.CompletedTask;
        }

        public Task Given_a_new_host_with_custom_ComponentContainer(IComponentContainer componentContainer)
        {
            CurrentHost = new REstateHost(componentContainer);

            return Task.CompletedTask;
        }

        public Task Then_no_exception_was_thrown()
        {
            Assert.Null(CurrentException);

            return Task.CompletedTask;
        }
    }
}