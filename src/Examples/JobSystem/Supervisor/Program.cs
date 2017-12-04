using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using REstate;
using REstate.Engine.Services;
using REstate.Remote;
using REstate.Schematics;
using Serilog;
using REstate.Engine.Repositories.Redis;
using StackExchange.Redis;

namespace Supervisor
{
    public enum JobStatus
    {
        Enqueued,
        Active,
        Suspended,
        Failed,
        Complete
    }

    public enum JobActions
    {
        Retry,
        Enqueued,
        Dequeued,
        Suspend,
        Fail,
        Complete
    }

    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .CreateLogger();

            REstateHost.Agent.Configuration.RegisterConnector(new ConnectorKey("Enqueue"), typeof(EnqueueConnector<,>));

            var connection = ConnectionMultiplexer.ConnectAsync(
                "restate.redis.cache.windows.net:6380," +
                "password=m7+cQKqV3x2KQaAOned8xGj62/0E2t0DrLk/KEElfH0=," +
                "ssl=True," +
                "abortConnect=False").Result;

            REstateHost.Agent.Configuration
                .RegisterComponent(new RedisRepositoryComponent(connection.GetDatabase()));

            REstateHost.Agent.Configuration
                .RegisterComponent(
                    new GrpcRemoteHostComponent(
                        new GrpcHostOptions
                        {
                            Channel = new Channel("localhost", 12345, ChannelCredentials.Insecure),
                            UseAsDefaultEngine = true
                        }));

            GrpcEnvironment.SetLogger(new ConsoleLogger());

            var server = REstateHost.Agent
                .AsRemote()
                .CreateGrpcServer(new ServerPort("localhost", 12345, ServerCredentials.Insecure));

            server.Start();


            var jobSchematic = REstateHost.Agent.CreateSchematic<JobStatus, JobActions>("Job")

                .WithState(JobStatus.Enqueued, state => state
                    .AsInitialState()
                    .DescribedAs("Job has been created or marked for retry, but has not been successfully put on the queue yet.")
                    .WithOnEntry(new ConnectorKey("Enqueue"), entry => entry
                        .DescribedAs("Enqueues the job on a queue.")
                        .OnFailureSend(JobActions.Suspend)))

                .WithState(JobStatus.Active, state => state
                    .DescribedAs("Job received from a worker.")
                    .WithTransitionFrom(JobStatus.Enqueued, JobActions.Dequeued))

                .WithState(JobStatus.Suspended, state => state
                    .DescribedAs("Worker or action requested a suspend and retry iteration.")
                    .WithTransitionFrom(JobStatus.Enqueued, JobActions.Suspend)
                    .WithTransitionFrom(JobStatus.Active, JobActions.Suspend))

                .WithState(JobStatus.Failed, state => state
                    .DescribedAs("Job encountered an error determined to be non-recoverable.")
                    .WithTransitionFrom(JobStatus.Active, JobActions.Fail)
                    .WithTransitionFrom(JobStatus.Suspended, JobActions.Fail)
                    .WithOnEntry(new ConnectorKey("Log"), entry => entry
                        .DescribedAs("Log the failure as an error.")
                        .WithSetting("severity", "error")
                        .WithSetting("message", "Job failed to complete.")))

                .WithState(JobStatus.Complete, entry => entry
                    .DescribedAs("Job has finished processing without errors.")
                    .WithTransitionFrom(JobStatus.Active, JobActions.Complete))

                .Build();

            var cancellationSource = new CancellationTokenSource();

            var jobHandler = EnqueueConnector<JobStatus, JobActions>.Queue;

            var jobEngine = REstateHost.Agent.GetStateEngine<JobStatus, JobActions>();

            var processor = Task.Run(() =>
            {
                ProcessJob(cancellationSource, jobHandler, jobEngine);
            });

            Console.ReadLine();

            var machine = jobEngine.CreateMachineAsync(jobSchematic, new Dictionary<string, string>
            {
                ["ServiceId"] = "066f5d62-8d36-4ad1-833e-9e874d0215f8",
                ["SubscriptionId"] = "f59447fc-277a-4dfd-baaf-f6fde6de1865"
            }).Result;

            var metadata = machine.GetMetadataAsync().Result;

            metadata = jobEngine.GetMachineAsync(machine.MachineId).Result.GetMetadataAsync().Result;

            Console.ReadLine();

            jobEngine.BulkCreateMachinesAsync(jobSchematic, GetAllSubscriptions()).Wait();

            Console.ReadLine();

            Console.WriteLine("Shutting down processor...");

            cancellationSource.Cancel();


            processor.Wait();
            server.ShutdownAsync().Wait();

            Console.WriteLine("Processor terminated.");

            Console.ReadLine();

            var diagram = jobSchematic.WriteStateMap();
            Console.WriteLine(diagram);

            Console.ReadLine();

        }

        private static void ProcessJob(CancellationTokenSource cancellationSource, BlockingCollection<Status<JobStatus>> jobHandler, IStateEngine<JobStatus, JobActions> jobEngine)
        {
            Log.Logger.Debug("Awaiting job message...");

            try
            {
                foreach (var message in jobHandler.GetConsumingEnumerable(cancellationSource.Token))
                {
                    var jobMachine = jobEngine.GetMachineAsync(message.MachineId).Result;

                    // Notify the job, we have the message.
                    var status = jobMachine.SendAsync(JobActions.Dequeued).Result;

                    if (status.State != JobStatus.Active)
                        continue;

                    try
                    {
                        // Job actions here...
                        jobMachine.SendAsync(JobActions.Complete, status.CommitTag).Wait();
                    }
                    // Exception was transient, suspend/retry flow
                    catch (Exception e) when (e.Message.Contains("Transient"))
                    {
                        jobMachine.SendAsync(JobActions.Suspend, e, status.CommitTag).Wait();
                    }
                    // Exception is unrecoverable, no need to retry.
                    catch (Exception e)
                    {
                        jobMachine.SendAsync(JobActions.Fail, e, status.CommitTag).Wait();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ends peacefully
            }
        }

        public static Dictionary<string, string>[] GetAllSubscriptions() =>
            Enumerable
            .Range(0, 200)
            .Select(i =>
                new Dictionary<string, string>
                {
                    ["ServiceId"] = Guid.NewGuid().ToString(),
                    ["SubscriptionId"] = Guid.NewGuid().ToString()
                })
            .ToArray();
    }

    public class EnqueueConnector<TState, TInput> 
        : IConnector<TState, TInput>
    {
        public static readonly BlockingCollection<Status<TState>> Queue =
            new BlockingCollection<Status<TState>>(new ConcurrentQueue<Status<TState>>());

        public ConnectorKey Key { get; } = new ConnectorKey("Enqueue");

        public Task OnEntryAsync<TPayload>(
            ISchematic<TState, TInput> schematic,
            IStateMachine<TState, TInput> machine,
            Status<TState> status,
            InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Queue.Add(status, cancellationToken);

            return Task.CompletedTask;
        }

        public Task<bool> GuardAsync<TPayload>(ISchematic<TState, TInput> schematic, IStateMachine<TState, TInput> machine, Status<TState> status, InputParameters<TInput, TPayload> inputParameters,
            IReadOnlyDictionary<string, string> connectorSettings, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotSupportedException();
        }

        public IBulkEntryConnector<TState, TInput> GetBulkEntryConnector()
        {
            throw new NotSupportedException();
        }
    }
}