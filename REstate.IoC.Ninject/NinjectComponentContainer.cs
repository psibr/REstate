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
            var binding = Kernel.Bind<T>().ToConstant(instance);

            if(name is null)
                return;

            binding.Named(name);
        }

        public void Register(Type registrationType, Type implementationType, string name = null)
        {
            var binding = Kernel.Bind(registrationType).To(implementationType);

            if(name is null)
                return;

            binding.Named(name);
        }

        public void Register<T>(FactoryMethod<T> resolver, string name = null) where T : class
        {
            T Resolve(IContext context) => resolver(this);

            var binding = Kernel.Bind<T>().ToMethod(Resolve);

            if(name is null)
                return;

            binding.Named(name);
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
            var binding = Kernel.Bind<TBinding>().To<TImplementation>();

            if(name is null)
                return;

            binding.Named(name);
        }
    }
}
