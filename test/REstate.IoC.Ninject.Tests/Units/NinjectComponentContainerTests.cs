using System.Collections.Generic;
using Ninject;
using REstate.Engine;
using REstate.Engine.Connectors;
using REstate.Schematics;
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
            var connectors = container.ResolveAll<IAction<string, string>>();

            // ASSERT
            Assert.NotNull(connectors);
            Assert.NotEmpty(connectors);
        }

        [Fact]
        public void CanOverrideABinding()
        {
            // ARRANGE
            var kernel = new StandardKernel();
            var container = new NinjectComponentContainer(kernel);

            // ReSharper disable once ObjectCreationAsStatement
            new REstateHost(container);

            // ACT
            container.Register(typeof(ICartographer<,>), typeof(TestCartographer<,>));
            var resolvedCartographer = container.Resolve<ICartographer<string, string>>();

            // ASSERT
            Assert.IsType<TestCartographer<string, string>>(resolvedCartographer);
        }

        [Fact]
        public void CanBindMultipleNamed()
        {
            // ARRANGE
            var kernel = new StandardKernel();
            var container = new NinjectComponentContainer(kernel);

            // ReSharper disable once ObjectCreationAsStatement
            new REstateHost(container);

            // ACT
            container.Register(typeof(ICartographer<,>), typeof(DotGraphCartographer<,>), "dot");
            container.Register(typeof(ICartographer<,>), typeof(TestCartographer<,>), "test");

            var test = container.Resolve<ICartographer<string, string>>("test");
            var dot = container.Resolve<ICartographer<string, string>>("dot");

            // ASSERT
            Assert.IsType<TestCartographer<string, string>>(test);
            Assert.IsType<DotGraphCartographer<string, string>>(dot);
        }
    }

    public class TestCartographer<TState, TInput>
        : ICartographer<TState, TInput>
    {
        public string WriteMap(IEnumerable<IState<TState, TInput>> states)
        {
            return "";
        }
    }
}
