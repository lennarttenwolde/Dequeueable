using Dequeueable.AzureQueueStorage.Models;

namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    internal interface IQueueMessageExecutor
    {
        Task ExecuteAsync(Message message, CancellationToken cancellationToken);
    }
}
