using Dequeueable.AzureQueueStorage.Models;

namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    internal sealed class QueueMessageExecutor(IAzureQueueFunction function) : IQueueMessageExecutor
    {
        public async Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            await function.ExecuteAsync(message, cancellationToken);
        }
    }
}
