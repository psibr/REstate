using System;
using System.Collections.Generic;
using System.Text;
using MagicOnion.Client;
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
            registrar.Register(container => 
                MagicOnionClient.Create<IStateMachineService>(_options.Channel));

            registrar.Register(typeof(IRemoteStateEngine<,>), typeof(GrpcStateEngine<,>));

            if(_options.UseAsDefaultEngine)
                registrar.Register(typeof(IStateEngine<,>), typeof(GrpcStateEngine<,>));
        }
    }
}
