using System;
using REstate.Engine;
using REstate.Remote.Services;
using REstate.IoC;

namespace REstate.Remote
{
    public class GrpcRemoteHostComponent
        : IComponent
    {
        private readonly GrpcHostOptions _options;

        public GrpcRemoteHostComponent(GrpcHostOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (options.Channel == null)
                throw new ArgumentNullException(nameof(options.Channel), $"Channel must be provided in {nameof(GrpcHostOptions)}.");

            _options = options;
        }

        /// <summary>
        /// Registers dependencies in the component.
        /// </summary>
        /// <param name="registrar">The registration system to register with, typically a DI container.</param>
        public void Register(IRegistrar registrar)
        {
            registrar.Register(_options);

            registrar.Register(typeof(IStateMachineServiceClient), typeof(StateMachineServiceClient));

            registrar.Register(container => container.Resolve<IStateMachineServiceClient>().Create());

            registrar.Register(typeof(IRemoteStateEngine<,>), typeof(GrpcStateEngine<,>));

            if(_options.UseAsDefaultEngine)
                registrar.Register(typeof(IStateEngine<,>), typeof(GrpcStateEngine<,>));

            registrar.Register((contianer) => contianer.Resolve<IAgent>().AsRemote());
        }
    }
}
