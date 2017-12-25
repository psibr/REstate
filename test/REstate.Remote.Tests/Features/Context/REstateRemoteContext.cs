using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using REstate.Tests.Features.Context;

namespace REstate.Remote.Tests.Features.Context
{
    using static SharedRemoteContext;

    public static class SharedRemoteContext
    {
        public static REstateGrpcServer CurrentGrpcServer { get; set; }

        public static object CurrentGrpcServerSyncRoot = new object();
    }

    public class REstateRemoteContext<TState, TInput>
        : REstateContext<TState, TInput>
    {
        public void Given_a_REstate_gRPC_Server_running_on_the_default_endpoint()
        {
            if(CurrentGrpcServer != null) return;

            lock (CurrentGrpcServerSyncRoot)
            {
                if(CurrentGrpcServer != null) return;

                CurrentGrpcServer = CurrentHost.Agent()
                    .AsRemote()
                    .CreateGrpcServer(new ServerPort("0.0.0.0", 12345, ServerCredentials.Insecure));

                CurrentGrpcServer.Start();
            }
            
        }

        public void Given_the_default_agent_is_gRPC_remote_on_default_endpoint()
        {
            CurrentHost.Agent().Configuration
                .RegisterComponent(new GrpcRemoteHostComponent(new GrpcHostOptions
                {
                    Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                    UseAsDefaultEngine = true
                }));
        }
    }
}
