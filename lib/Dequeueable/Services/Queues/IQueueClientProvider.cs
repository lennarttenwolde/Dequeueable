using Azure.Storage.Queues;

namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    /// <summary>
    /// Provides the <see cref="QueueClient"/> used for communicating with the queue. Can be overriden if custom implementation is needed.
    /// </summary>
    public interface IQueueClientProvider
    {
        /// <summary>
        /// Gets the <see cref="QueueClient"/> used for communicating with the queue.
        /// </summary>
        /// <returns>A <see cref="QueueClient"/></returns>
        QueueClient GetQueue();

        /// <summary>
        /// Gets the <see cref="QueueClient"/> used for communicating with the queue.
        /// </summary>
        /// <returns>A <see cref="QueueClient"/></returns>
        QueueClient GetPoisonQueue();
    }
}