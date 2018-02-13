using Ninject;
using REstate.Engine.Connectors;
using Xunit;

namespace REstate.IoC.Ninject.Tests.Units
{
    public class NinjectComponentContainerTests
    {
        [Fact]
        public void CanResolveMultiple()
        {
            // ARRANGE
            var container = new NinjectComponentContainer(new StandardKernel());

            // ReSharper disable once ObjectCreationAsStatement
            new REstateHost(container);

            // ACT
            var connectors = container.ResolveAll<IEntryConnector<string, string>>();

            // ASSERT
            Assert.NotNull(connectors);
            Assert.NotEmpty(connectors);
        }
    }
}
