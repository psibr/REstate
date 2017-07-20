using REstate;
using REstate.IoC;

namespace Scratchpad
{
    public class InMemoryStateVisorComponent
        : IComponent
    {
        private readonly InMemoryStateVisor _visor;

        public InMemoryStateVisorComponent(InMemoryStateVisor visor)
        {
            _visor = visor;
        }

        /// <summary>
        /// Registers dependencies in the component.
        /// </summary>
        /// <param name="registrar">The registration system to register with, typically a DI container.</param>
        public void Register(IRegistrar registrar)
        {
            registrar.Register<IEventListener>(_visor, nameof(InMemoryStateVisor));
        }
    }
}