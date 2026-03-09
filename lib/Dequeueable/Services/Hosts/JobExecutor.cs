using Dequeueable.Models;
using Dequeueable.Services.Queues;
using Microsoft.Extensions.Logging;

namespace Dequeueable.Services.Hosts
{
    internal sealed class JobExecutor(
        IQueueMessageManager messagesManager,
        IQueueMessageHandler queueMessageHandler,
        ILogger<JobExecutor> logger) : IJobExecutor
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {

                var message = await messagesManager.RetrieveMessageAsync(cancellationToken);

                if (message is not null)
                {
                    await queueMessageHandler.HandleAsync(message, cancellationToken);
                }
                else
                {
                    logger.LogDebug("No messages found");
                }
            }
            catch (Exception ex) when (ex is not TaskCanceledException)
            {
                logger.LogError(ex, "Unhandled exception occurred, unable to process the message.");
                throw;
            }

        }
    }
}
