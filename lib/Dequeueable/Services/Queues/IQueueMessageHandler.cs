using Dequeueable.AzureQueueStorage.Models;

namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    internal interface IQueueMessageHandler
    {
        Task HandleAsync(Message message, CancellationToken cancellationToken);
    }
}
