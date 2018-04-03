using System;
using System.Linq;
using Ninject;
using Ninject.Activation;

namespace REstate.IoC.Ninject
{
    public class NinjectComponentContainer
        : IComponentContainer
    {
        public NinjectComponentContainer(IKernel kernel)
        {
            Kernel = kernel;
        }

        protected IKernel Kernel { get; }
        
        public void Register<T>(T instance, string name = null) where T : class
        {
            if (name is null)
                Kernel.Rebind<T>().ToConstant(instance).InSingletonScope();
            else
                Kernel.Bind<T>().ToConstant(instance).InSingletonScope().Named(name);
        }

        public void Register(Type registrationType, Type implementationType, string name = null)
        {
            if (name is null)
                Kernel.Rebind(registrationType).To(implementationType).InSingletonScope();
            else
                Kernel.Bind(registrationType).To(implementationType).InSingletonScope().Named(name);
        }

        public void Register<T>(FactoryMethod<T> resolver, string name = null) where T : class
        {
            T Resolve(IContext context) => resolver(this);

            if (name is null)
                Kernel.Rebind<T>().ToMethod(Resolve).InSingletonScope();
            else
                Kernel.Bind<T>().ToMethod(Resolve).InSingletonScope().Named(name);
        }

        public void RegisterComponent(IComponent component)
        {
            component.Register(this);
        }

        public T Resolve<T>(string name = null) where T : class 
            => name is null ? Kernel.Get<T>() : Kernel.Get<T>(name);

        public T[] ResolveAll<T>() where T : class => 
            Kernel.GetAll<T>().ToArray();

        void IRegistrar.Register<TBinding, TImplementation>(string name)
        {
            if (name is null)
                 Kernel.Rebind<TBinding>().To<TImplementation>().InSingletonScope();
            else
                Kernel.Bind<TBinding>().To<TImplementation>().InSingletonScope().Named(name);
        }
    }
}
