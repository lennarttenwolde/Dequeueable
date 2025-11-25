using Dequeueable.AmazonSQS.Services.Queues;
using Microsoft.Extensions.Logging;

namespace Dequeueable.AmazonSQS.Services.Hosts
{
    internal sealed class JobExecutor(IQueueMessageManager queueMessageManager, IQueueMessageHandler queueMessageHandler, ILogger<JobExecutor> logger) : IHostExecutor
    {
        private readonly List<Task> _processing = [];

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            var messages = await queueMessageManager.RetrieveMessagesAsync(cancellationToken: cancellationToken);
            var messagesFound = messages.Length > 0;
            if (messagesFound)
            {
                await HandleMessages(messages!, cancellationToken);
            }
            else
            {
                logger.LogDebug("No messages found");
            }

            return;
        }

        private Task HandleMessages(Models.Message[] messages, CancellationToken cancellationToken)
        {
            foreach (var message in messages)
            {
                var task = queueMessageHandler.HandleAsync(message, cancellationToken);
                _processing.Add(task);
            }

            return Task.WhenAll(_processing);
        }
    }
}
