using System;
using System.Threading.Tasks;
using REstate;
using Serilog;

namespace Semaphore
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            Log.Logger =
                new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .CreateLogger();

            var sempahore = await REstateHost.Agent
                .CreateSemaphoreAsync(3);

            var task1 = Task.Run(() => DoSomeProcessingAsync(60));
            var task2 = Task.Run(() => DoSomeProcessingAsync(40));
            var task3 = Task.Run(() => DoSomeProcessingAsync(20));
            var task4 = Task.Run(() => DoSomeProcessingAsync(50));
            var task5 = Task.Run(() => DoSomeProcessingAsync(10));

            await Task.WhenAll(task1, task2, task3, task4, task5);

            Log.Logger.Information("Done!");
            Console.ReadLine();

            async Task DoSomeProcessingAsync(int workPeriodMs, int onFailedToGetSlotDelayMs = 5)
            {
                while (true)
                {
                    try
                    {
                        using (await sempahore.EnterAsync())
                        {
                            await Task.Delay(workPeriodMs);
                        }

                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        Log.Logger.Verbose("Semapore is full, waiting {delay}ms and then retrying...",
                            onFailedToGetSlotDelayMs);

                        await Task.Delay(onFailedToGetSlotDelayMs);
                    }
                }
            }
        }
    }
}
