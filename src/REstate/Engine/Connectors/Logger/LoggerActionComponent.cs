using REstate.IoC;

namespace REstate.Engine.Connectors.Logger
{
    public class LoggingActionComponent
        : IComponent
    {
        /// <summary>
        /// Registers dependencies in the component.
        /// </summary>
        /// <param name="registrar">The registration system to register with, typically a DI container.</param>
        public void Register(IRegistrar registrar)
        {
            registrar.RegisterConnector(typeof(LoggingAction<,>))
                .WithConfiguration(new LoggingActionConfiguration("log trace") { LogLevel = LogLevel.Trace })
                .WithConfiguration(new LoggingActionConfiguration("log debug") { LogLevel = LogLevel.Debug })
                .WithConfiguration(new LoggingActionConfiguration("log info") { LogLevel = LogLevel.Info })
                .WithConfiguration(new LoggingActionConfiguration("log warn") { LogLevel = LogLevel.Warn })
                .WithConfiguration(new LoggingActionConfiguration("log error") { LogLevel = LogLevel.Error })
                .WithConfiguration(new LoggingActionConfiguration("log fatal") { LogLevel = LogLevel.Fatal });
        }
    }
}