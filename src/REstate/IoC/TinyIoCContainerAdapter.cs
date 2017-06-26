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
        /// <param name="container">The container.</param>
        internal TinyIoCContainerAdapter(TinyIoCContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Resolves the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns>T.</returns>
        /// <exception cref="SusanooDependencyResolutionException">An error occured resolving a type.</exception>
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
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
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
        /// <param name="resolver">The resolver.</param>
        /// <param name="name">The name.</param>
        public void Register<T>(Func<IComponentContainer, T> resolver, string name = null)
            where T : class
        {
            if (name == null)
                _container.Register((tinyIoC, overloads) => resolver(this));
            else
                _container.Register((tinyIoC, overloads) => resolver(this), name);
        }

        public void RegisterComponent(IComponent component)
        {
            component.Register(this);
        }
    }
}