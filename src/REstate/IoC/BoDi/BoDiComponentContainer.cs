using System;
using System.Linq;

namespace REstate.IoC.BoDi
{
    internal class BoDiComponentContainer 
        : IComponentContainer
    {
        private readonly ObjectContainer _container;

        public BoDiComponentContainer(ObjectContainer container)
        {
            _container = container;
        }

        public void Register<T>(T instance, string name = null) where T : class
        {
            _container.RegisterInstanceAs(instance, name);
        }

        public void Register<TBinding, TImplementation>(string name = null) 
            where TBinding : class 
            where TImplementation : class, TBinding
        {
            _container.RegisterTypeAs<TBinding>(typeof(TImplementation), name);
        }

        public void Register(Type registrationType, Type implementationType, string name = null)
        {
            _container.RegisterTypeAs(implementationType, registrationType, name);
        }

        public void Register<T>(Func<IComponentContainer, T> resolver, string name = null) where T : class
        {
            _container.RegisterFactoryAs(() => resolver(this), name);
        }

        public void RegisterComponent(IComponent component)
        {
            component.Register(this);
        }

        public T Resolve<T>(string name = null) 
            where T : class =>
            _container.Resolve<T>(name);

        public T[] ResolveAll<T>() where T : class
        {
            return _container.ResolveAll<T>().ToArray();
        }
    }
}