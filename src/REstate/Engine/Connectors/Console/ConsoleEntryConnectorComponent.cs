using REstate.IoC;

namespace REstate.Engine.Connectors.Console
{
    public class ConsoleEntryConnectorComponent
        : IComponent
    {
        /// <summary>
        /// Registers dependencies in the component.
        /// </summary>
        /// <param name="registrar">The registration system to register with, typically a DI container.</param>
        public void Register(IRegistrar registrar)
        {
            registrar.RegisterConnector(typeof(ConsoleEntryConnector<,>))
                .WithConfiguration(new ConsoleEntryConnectorConfiguration("console writeline"))
                .WithConfiguration(new ConsoleEntryConnectorConfiguration("console write") { Mode = ConsoleWriteMode.Write });
        }
    }
}