using Azure.Core;
using Azure.Storage.Queues;

namespace Dequeueable.AzureQueueStorage.Factories
{
    internal sealed class QueueClientFactory : IQueueClientFactory
    {
        public QueueClient Create(string connectionString, string queueName, QueueClientOptions? queueClientOptions = null)
        {
            return new QueueClient(connectionString, queueName, queueClientOptions);
        }

        public QueueClient Create(Uri uri, TokenCredential tokenCredential, QueueClientOptions? queueClientOptions = null)
        {
            return new QueueClient(uri, tokenCredential, queueClientOptions);
        }
    }
}
