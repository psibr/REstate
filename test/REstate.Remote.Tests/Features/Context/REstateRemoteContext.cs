using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using REstate.Tests.Features.Context;

namespace REstate.Remote.Tests.Features.Context
{
    public partial class REstateRemoteContext<TState, TInput>
        : REstateContext<TState, TInput>
    {
        public REstateGrpcServer CurrentRemoteServer { get; set; }

        public void Given_a_REstate_RemoteServer_on_default_endpoint()
        {
            CurrentRemoteServer = CurrentHost
                .Agent().AsRemote()
                .CreateGrpcServer(new ServerPort("0.0.0.0", 12345, ServerCredentials.Insecure));

            CurrentRemoteServer.Start();
        }

        public void Given_RemoteAgent_is_default_on_default_endpoint()
        {
            CurrentHost.Agent().Configuration
                .RegisterComponent(new GrpcRemoteHostComponent(
                    new GrpcHostOptions
                    {
                        Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                        UseAsDefaultEngine = true
                    }));
        }

        public void Given_RemoteAgent_is_registered_on_default_endpoint_but_not_default()
        {
            CurrentHost.Agent().Configuration
                .RegisterComponent(new GrpcRemoteHostComponent(
                    new GrpcHostOptions
                    {
                        Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                        UseAsDefaultEngine = false
                    }));
        }
    }
}
