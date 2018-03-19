namespace REstate.IoC
{
    /// <summary>
    /// A single dependency, or set of dependencies to register.
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// Registers dependencies in the component.
        /// </summary>
        /// <param name="registrar">The registration system to register with, typically a DI container.</param>
        void Register(IRegistrar registrar);
    }
}
