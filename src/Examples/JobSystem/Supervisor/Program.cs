using System;
using System.Threading;
using Grpc.Core;
using Grpc.Core.Logging;
using REstate;

namespace Supervisor
{
    class Program
    {
        public enum JobStatus
        {
            ReadyToQueue = 0,

            Scheduled = 1,
            Active = 2,
            Suspended = 3,
            Failed = 4,
            Complete = 5
        }

        public enum JobActions
        {
            Retry = 0,
            Enqueued = 1,
            Dequeued = 2,
            Suspend = 3,
            Fail = 4,
            Complete = 5
        }

        static void Main(string[] args)
        {
            var jobDiagram = REstateHost.Agent
                .CreateSchematic<JobStatus, JobActions>("Job")
                .WithState(JobStatus.ReadyToQueue, state => state
                    .AsInitialState()
                    .DescribedAs("Job has been created or marked for retry, but has not been successfully put on the queue yet.")
                    .WithOnEntry("AzureStorageQueue", entry => entry
                        .DescribedAs("Enqueues the job on an Azure Storage Queue.")
                        .WithSetting("endpoint", "")
                        .OnFailureSend(JobActions.Suspend)))
                .WithState(JobStatus.Scheduled, state => state
                    .DescribedAs("Jobs has been placed on the queue.")
                    .WithTransitionFrom(JobStatus.ReadyToQueue, JobActions.Enqueued))
                .WithState(JobStatus.Active, state => state
                    .DescribedAs("Job received from a worker.")
                    .WithTransitionFrom(JobStatus.Scheduled, JobActions.Dequeued))
                .WithState(JobStatus.Suspended, state => state
                    .DescribedAs("Worker or action requested a suspend and retry iteration.")
                    .WithTransitionFrom(JobStatus.ReadyToQueue, JobActions.Suspend)
                    .WithTransitionFrom(JobStatus.Active, JobActions.Suspend, transition => transition
                        .WithGuard("SuspendRetryCounter", guard => guard
                            .DescribedAs(
                                "Keeps track of whether a suspension is valid after after so many retry iterations.")
                            .WithSetting("RetryCount", "3")))
                    .WithTransitionTo(JobStatus.ReadyToQueue, JobActions.Retry))
                .WithState(JobStatus.Failed, state => state
                    .DescribedAs("Job encountered an error determined to be non-recoverable.")
                    .WithTransitionFrom(JobStatus.Active, JobActions.Fail)
                    .WithOnEntry("CriticalLogger", entry => entry
                        .DescribedAs("Log the failure as a critical error.")))
                .WithState(JobStatus.Complete, entry => entry
                    .DescribedAs("Job has finished processing without errors.")
                    .WithTransitionFrom(JobStatus.Active, JobActions.Complete))
                .Copy()
                .WriteStateMap();


            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var server = REstateHost.Agent
                .AsRemote()
                .CreateGrpcServer(new ServerPort("localhost", 12345, ServerCredentials.Insecure));

            // launch gRPC Server.
            server.Start();

            SpinWait.SpinUntil(() => server.ShutdownTask.IsCompleted);

            Console.ReadLine();
        }
    }
}