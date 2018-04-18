using REstate.Engine.EventListeners;

namespace REstate.IoC
{
    public static class RegistrarEventListenerExtensions
    {
        public static void RegisterEventListener(this IRegistrar registrar, IEventListener listener)
        {
            registrar.Register(listener, listener.GetType().AssemblyQualifiedName);
        }
    }
}