using Dequeueable.AzureQueueStorage.Models;

namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    internal interface IQueueMessageManager
    {
        Task DeleteMessageAsync(Message queueMessage, CancellationToken cancellationToken);
        Task EnqueueMessageAsync(Message queueMessage, CancellationToken cancellationToken);
        Task MoveToPoisonQueueAsync(Message queueMessage, CancellationToken cancellationToken);
        Task<IEnumerable<Message>> RetrieveMessagesAsync(CancellationToken cancellationToken);
        Task<DateTimeOffset> UpdateVisibilityTimeOutAsync(Message queueMessage, CancellationToken cancellationToken);
    }
}