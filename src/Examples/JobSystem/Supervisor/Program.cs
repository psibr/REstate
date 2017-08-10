using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using REstate;
using REstate.Engine.Services;
using REstate.Schematics;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Supervisor
{
    class Program
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

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            REstateHost.Agent.Configuration.RegisterConnector("Enqueue", typeof(EnqueueConnector<,>));

            var jobSchematic = REstateHost.Agent.CreateSchematic<JobStatus, JobActions>("Job")

                .WithState(JobStatus.Enqueued, state => state
                    .AsInitialState()
                    .DescribedAs("Job has been created or marked for retry, but has not been successfully put on the queue yet.")
                    .WithOnEntry("Enqueue", entry => entry
                        .DescribedAs("Enqueues the job on a queue.")
                        .OnFailureSend(JobActions.Suspend)))

                .WithState(JobStatus.Active, state => state
                    .DescribedAs("Job received from a worker.")
                    .WithTransitionFrom(JobStatus.Enqueued, JobActions.Dequeued))

                .WithState(JobStatus.Suspended, state => state
                    .DescribedAs("Worker or action requested a suspend and retry iteration.")
                    .WithTransitionFrom(JobStatus.Enqueued, JobActions.Suspend)
                    .WithTransitionFrom(JobStatus.Active, JobActions.Suspend)
                    .WithTransitionTo(JobStatus.Enqueued, JobActions.Retry)
                    .WithOnEntry("Retry", entry => entry
                        .DescribedAs("Retries the job a specified number of times before failing.")
                        .WithSetting("retryIterations", "3")
                        .OnFailureSend(JobActions.Fail)))

                .WithState(JobStatus.Failed, state => state
                    .DescribedAs("Job encountered an error determined to be non-recoverable.")
                    .WithTransitionFrom(JobStatus.Active, JobActions.Fail)
                    .WithTransitionFrom(JobStatus.Suspended, JobActions.Fail)
                    .WithOnEntry("Log", entry => entry
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

            jobEngine.CreateMachineAsync(jobSchematic, new Dictionary<string, string>
            {
                ["ServiceId"] = "066f5d62-8d36-4ad1-833e-9e874d0215f8",
                ["SubscriptionId"] = "f59447fc-277a-4dfd-baaf-f6fde6de1865"
            }).Wait();

            Console.ReadLine();

            jobEngine.BulkCreateMachinesAsync(jobSchematic, GetAllSubscriptions()).Wait();

            Console.ReadLine();

            Console.WriteLine("Shutting down processor...");

            cancellationSource.Cancel();


            processor.Wait();

            Console.WriteLine("Processor terminated.");

            Console.ReadLine();

            var diagram = jobSchematic.WriteStateMap();
            Console.WriteLine(diagram);

            Console.ReadLine();

        }

        private static void ProcessJob(CancellationTokenSource cancellationSource, BlockingCollection<Status<JobStatus>> jobHandler, IStateEngine<JobStatus, JobActions> jobEngine)
        {
            while (true)
            {
                Status<JobStatus> message;

                Log.Logger.Debug("Awaiting job message...");

                try
                {
                    message = jobHandler.Take(cancellationSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

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

    public class EnqueueConnector<TState, TInput> : IConnector<TState, TInput>
    {
        public static readonly BlockingCollection<Status<TState>> Queue =
            new BlockingCollection<Status<TState>>(new ConcurrentQueue<Status<TState>>());

        public string ConnectorKey { get; } = "Enqueue";

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