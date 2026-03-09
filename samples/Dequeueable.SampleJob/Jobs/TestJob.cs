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

                logger.ExecutingJobLoop(i);

                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation("Job cancelled!");
                    break;
                }
                await Task.Delay(10000, cancellationToken);
            }
        }
    }

    internal static partial class Logs
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Executing job loop {I}")]
        internal static partial void ExecutingJobLoop(this ILogger<TestJob> logger, int i);
    }
}
