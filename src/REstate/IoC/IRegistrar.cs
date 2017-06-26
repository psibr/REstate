using System;

namespace REstate.IoC
{
    public interface IRegistrar
    {
        /// <summary>
        /// Registers the specified instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        void Register<T>(T instance, string name = null)
            where T : class;

        /// <summary>
        /// Registers the specified factory method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resolver">The resolver.</param>
        /// <param name="name">The name.</param>
        void Register<T>(Func<IComponentContainer, T> resolver, string name = null)
            where T : class;

        /// <summary>
        /// Registers a component and all of it's associated dependencies.
        /// </summary>
        /// <param name="component">The component to register.</param>
        void RegisterComponent(IComponent component);
    }

}