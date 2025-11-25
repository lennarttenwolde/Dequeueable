using Dequeueable.AmazonSQS.Configurations;
using Dequeueable.AmazonSQS.Services.Queues;
using Dequeueable.AmazonSQS.Services.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dequeueable.AmazonSQS.Services.Hosts
{
    internal sealed class QueueListenerExecutor(IQueueMessageManager queueMessageManager,
        IQueueMessageHandler queueMessageHandler,
        IOptions<ListenerHostOptions> options,
        ILogger<QueueListenerExecutor> logger) : IHostExecutor
    {
        private readonly ListenerHostOptions _options = options.Value;
        private readonly List<Task> _processing = [];

        private RandomizedExponentialDelayStrategy DelayStrategy => new(TimeSpan.FromMilliseconds(_options.MinimumPollingIntervalInMilliseconds),
                TimeSpan.FromMilliseconds(_options.MaximumPollingIntervalInMilliseconds),
                _options.DeltaBackOff);

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

            await WaitForDelay(messagesFound, cancellationToken);
        }

        private Task HandleMessages(Models.Message[] messages, CancellationToken cancellationToken)
        {
            foreach (var message in messages)
            {
                var task = queueMessageHandler.HandleAsync(message, cancellationToken);
                _processing.Add(task);
            }

            return WaitForNewBatchThreshold(cancellationToken);
        }

        private Task WaitForDelay(bool messageFound, CancellationToken cancellationToken)
        {
            var delay = DelayStrategy.GetNextDelay(executionSucceeded: messageFound);
            return Task.Delay(delay, cancellationToken);
        }

        private async Task WaitForNewBatchThreshold(CancellationToken cancellationToken)
        {
            var newBatchThreshold = _options.NewBatchThreshold ?? Convert.ToInt32(Math.Ceiling(_options.BatchSize / (double)2));

            while (_processing.Count > newBatchThreshold && !cancellationToken.IsCancellationRequested)
            {
                var processed = await Task.WhenAny(_processing);
                _processing.Remove(processed);
            }
        }
    }
}
