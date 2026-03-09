using Dequeueable.Models;

namespace Dequeueable.Services.Queues
{
    internal interface IQueueMessageManager
    {
        Task DeleteMessageAsync(Message queueMessage, CancellationToken cancellationToken);
        Task EnqueueMessageAsync(Message queueMessage, CancellationToken cancellationToken);
        Task MoveToPoisonQueueAsync(Message queueMessage, CancellationToken cancellationToken);
        Task<Message?> RetrieveMessageAsync(CancellationToken cancellationToken);
        Task<DateTimeOffset> UpdateVisibilityTimeOutAsync(Message queueMessage, CancellationToken cancellationToken);
    }
}