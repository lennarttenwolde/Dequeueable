using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Models;

namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    internal sealed class QueueMessageManager(IQueueClientProvider queueClientProvider, IHostOptions options) : IQueueMessageManager
    {
        private readonly QueueClient _queueClient = queueClientProvider.GetQueue();
        private readonly QueueClient _poisonQueueClient = queueClientProvider.GetPoisonQueue();

        public async Task<IEnumerable<Message>> RetrieveMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _queueClient.ReceiveMessagesAsync(maxMessages: options.BatchSize, visibilityTimeout: TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), cancellationToken);
                return response.Value.Select(m => new Message(m.MessageId, m.PopReceipt, m.DequeueCount, m.NextVisibleOn, m.Body));
            }
            catch (RequestFailedException exception) when (exception.Status == 404)
            {
                await CreateQueue(_queueClient, cancellationToken);
                var response = await _queueClient.ReceiveMessagesAsync(maxMessages: options.BatchSize, visibilityTimeout: TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), cancellationToken);
                return response.Value.Select(m => new Message(m.MessageId, m.PopReceipt, m.DequeueCount, m.NextVisibleOn, m.Body));
            }
        }

        public async Task<DateTimeOffset> UpdateVisibilityTimeOutAsync(Message queueMessage, CancellationToken cancellationToken)
        {
            var retryInterval = TimeSpan.FromSeconds(5);
            var retryDeadline = queueMessage.NextVisibleOn.HasValue ? queueMessage.NextVisibleOn.Value.Add(retryInterval) : DateTimeOffset.UtcNow;

            do
            {
                try
                {
                    var updateReceipt = (await _queueClient.UpdateMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, visibilityTimeout: TimeSpan.FromSeconds(options.VisibilityTimeoutInSeconds), cancellationToken: cancellationToken)).Value;
                    queueMessage.PopReceipt = updateReceipt.PopReceipt;
                    return updateReceipt.NextVisibleOn;
                }
                catch (RequestFailedException exception) when (exception.Status != 404)
                {
                    await Task.Delay(retryInterval, cancellationToken);
                }
            }
            while (DateTimeOffset.UtcNow < retryDeadline);

            throw new VisibilityTimeoutException("Unable to update the visibility timeout, retry deadline reached");
        }

        public async Task DeleteMessageAsync(Message queueMessage, CancellationToken cancellationToken)
        {
            try
            {
                await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
            }
            catch (RequestFailedException exception) when (exception.Status == 404)
            {
            }
        }

        public async Task EnqueueMessageAsync(Message queueMessage, CancellationToken cancellationToken)
        {
            await _queueClient.UpdateMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, queueMessage.Body, TimeSpan.Zero, cancellationToken);
        }

        public async Task MoveToPoisonQueueAsync(Message queueMessage, CancellationToken cancellationToken)
        {
            await SendMessageAsync(_poisonQueueClient, queueMessage, cancellationToken);
            await DeleteMessageAsync(queueMessage, cancellationToken);
        }

        private static async Task<SendReceipt?> SendMessageAsync(QueueClient queueClient, Message queueMessage, CancellationToken cancellationToken)
        {
            try
            {
                return (await queueClient.SendMessageAsync(queueMessage.Body, cancellationToken: cancellationToken)).Value;
            }
            catch (RequestFailedException exception) when (exception.Status == 404)
            {
                await CreateQueue(queueClient, cancellationToken);
                return (await queueClient.SendMessageAsync(queueMessage.Body, cancellationToken: cancellationToken)).Value;
            }
        }

        private static Task<Response> CreateQueue(QueueClient queueClient, CancellationToken cancellationToken)
        {
            return queueClient.CreateAsync(cancellationToken: cancellationToken);
        }
    }
}
