using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using REstate.Tests.Features.Context;
using Xunit;

namespace REstate.Remote.Tests.Features.Context
{
    using static SharedRemoteContext;

    public static class SharedRemoteContext
    {
        public static REstateGrpcServer CurrentGrpcServer { get; set; }

        public static readonly object CurrentGrpcServerSyncRoot = new object();
    }

    public class REstateRemoteContext<TState, TInput>
        : REstateContext<TState, TInput>
    {
        public void Given_a_REstate_gRPC_Server_running()
        {
            if (CurrentGrpcServer == null)
            {
                lock (CurrentGrpcServerSyncRoot)
                {
                    if (CurrentGrpcServer == null)
                    {
                        CurrentGrpcServer = CurrentHost.Agent()
                            .AsRemote()
                            .CreateGrpcServer(new ServerPort("0.0.0.0", 0, ServerCredentials.Insecure));
                    }
                }
            }

            CurrentGrpcServer.Start();
        }

        public void When_a_REstate_gRPC_Server_is_created_and_started()
        {
            if (CurrentGrpcServer == null)
            {
                lock (CurrentGrpcServerSyncRoot)
                {
                    if (CurrentGrpcServer == null)
                    {
                        CurrentGrpcServer = CurrentHost.Agent()
                            .AsRemote()
                            .CreateGrpcServer(new ServerPort("0.0.0.0", 0, ServerCredentials.Insecure));
                    }
                }
            }

            CurrentGrpcServer.Start();
        }

        public void Given_the_default_agent_is_gRPC_remote()
        {
            CurrentHost.Agent().Configuration
                .RegisterComponent(new GrpcRemoteHostComponent(new GrpcHostOptions
                {
                    Channel = new Channel("localhost", CurrentGrpcServer.BoundPorts[0], ChannelCredentials.Insecure),
                    UseAsDefaultEngine = true
                }));
        }

        public void Then_REstate_gRPC_Server_has_bound_ports()
        {
            Assert.NotNull(CurrentGrpcServer);
            Assert.NotEmpty(CurrentGrpcServer.BoundPorts);
            Assert.DoesNotContain(0, CurrentGrpcServer.BoundPorts);
        }

        public void Given_a_REstate_gRPC_Server_failure()
        {
            CurrentGrpcServer.KillAsync().GetAwaiter().GetResult();
        }
    }
}
