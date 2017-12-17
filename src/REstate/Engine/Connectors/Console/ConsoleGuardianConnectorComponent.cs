using REstate.IoC;

namespace REstate.Engine.Connectors.Console
{
    public class ConsoleGuardianConnectorComponent
        : IComponent
    {
        /// <summary>
        /// Registers dependencies in the component.
        /// </summary>
        /// <param name="registrar">The registration system to register with, typically a DI container.</param>
        public void Register(IRegistrar registrar)
        {
            registrar.RegisterConnector(typeof(ConsoleGuardianConnector<,>))
                .WithConfiguration(new ConsoleGuardianConnectorConfiguration("console readline"))
                .WithConfiguration(new ConsoleGuardianConnectorConfiguration("console readkey") { Mode = ConsoleReadMode.ReadKey });
        }
    }
}