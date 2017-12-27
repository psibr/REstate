using System;
using REstate.IoC;
using Xunit;

namespace REstate.Tests.Features.Context
{
    public partial class REstateContext
    {
        public REstateHost CurrentHost { get; set; }

        public Exception CurrentException { get; set; }

        public void Given_a_new_host()
        {
            CurrentHost = new REstateHost();
        }

        public void Given_a_new_host_with_custom_ComponentContainer(IComponentContainer componentContainer)
        {
            CurrentHost = new REstateHost(componentContainer);
        }

        public void Then_no_exception_was_thrown()
        {
            Assert.Null(CurrentException);
        }
    }
}