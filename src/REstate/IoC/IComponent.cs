namespace REstate.IoC
{
    public interface IComponent
    {
        /// <summary>
        /// Registers dependencies.
        /// </summary>
        /// <param name="container"></param>
        void Register(IRegistrar registrar);
    }

}