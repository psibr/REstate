using REstate.IoC;

namespace REstate.Engine.Connectors.Logger
{
    public class LoggerEntryConnectorComponent
        : IComponent
    {
        /// <summary>
        /// Registers dependencies in the component.
        /// </summary>
        /// <param name="registrar">The registration system to register with, typically a DI container.</param>
        public void Register(IRegistrar registrar)
        {
            registrar.RegisterConnector(typeof(LoggerEntryConnector<,>))
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log trace") { LogLevel = LogLevel.Trace })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log debug") { LogLevel = LogLevel.Debug })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log info") { LogLevel = LogLevel.Info })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log warn") { LogLevel = LogLevel.Warn })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log error") { LogLevel = LogLevel.Error })
                .WithConfiguration(new LoggerEntryConnectorConfiguration("log fatal") { LogLevel = LogLevel.Fatal });
        }
    }
}