namespace REstate.IoC
{
    /// <summary>
    /// A simple container interface for plugging in DI containers.
    /// </summary>
    public interface IComponentContainer : IRegistrar
    {
        /// <summary>
        /// Resolves the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns>T.</returns>
        T Resolve<T>(string name = null)
            where T : class;

        /// <summary>
        /// Resolves all instances of a given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>An array of all the instances.</returns>
        T[] ResolveAll<T>()
            where T : class;
    }

}