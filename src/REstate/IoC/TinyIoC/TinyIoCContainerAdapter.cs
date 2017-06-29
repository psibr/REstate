using System;
using System.Linq;

namespace REstate.IoC.TinyIoC
{
    /// <summary>
    /// An adpater for TinyIoC to the shared IComponentContainer interface.
    /// </summary>
    internal class TinyIoCContainerAdapter
        : IComponentContainer
    {
        private readonly TinyIoCContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="TinyIoCContainerAdapter"/> class.
        /// </summary>
        /// <param name="container">The TinyIoC container.</param>
        internal TinyIoCContainerAdapter(TinyIoCContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Resolves the specified type.
        /// </summary>
        /// <typeparam name="T">The type requested for resolution.</typeparam>
        /// <param name="name">The name under which an implmentation was registered.</param>
        /// <returns>The type requested.</returns>
        public T Resolve<T>(string name = null)
            where T : class
        {
            return name == null
                ? _container.Resolve<T>()
                : _container.Resolve<T>(name);
        }

        /// <summary>
        /// Resolves all instances of a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An array of all the instances.</returns>
        public T[] ResolveAll<T>() 
            where T : class
        {
            return _container.ResolveAll<T>().ToArray();
        }

        /// <summary>
        /// Registers the specified instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance to register.</param>
        /// <param name="name">The name under which an implmentation was registered.</param>
        public void Register<T>(T instance, string name = null)
            where T : class
        {
            if (name == null)
                _container.Register(instance);
            else
                _container.Register(instance, name);
        }

        /// <summary>
        /// Registers the specified factory method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resolver">The resolver method that will build an implementation when requested.</param>
        /// <param name="name">The name under which an implmentation was registered.</param>
        public void Register<T>(Func<IComponentContainer, T> resolver, string name = null)
            where T : class
        {
            if (name == null)
                _container.Register((tinyIoC, overloads) => resolver(this));
            else
                _container.Register((tinyIoC, overloads) => resolver(this), name);
        }

        /// <summary>
        /// Registers a component and all of it's associated dependencies.
        /// </summary>
        /// <param name="component">The component to register.</param>
        public void RegisterComponent(IComponent component)
        {
            component.Register(this);
        }
    }
}