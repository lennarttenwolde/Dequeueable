using Dequeueable.Models;
using Microsoft.Extensions.Logging;

namespace Dequeueable.SampleJob.Jobs
{
    internal sealed class TestJob(ILogger<TestJob> logger) : IQueueJob
    {
        public async Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            for (var i = 0; i < 6; i++)
            {
#pragma warning disable CA1873 // Avoid potentially expensive logging
                logger.LogInformation("Executing job loop {I}", i);
#pragma warning restore CA1873 // Avoid potentially expensive logging
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Job cancelled!");
                    break;
                }
                await Task.Delay(10000, cancellationToken);
            }
        }
    }
}
