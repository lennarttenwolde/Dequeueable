using Dequeueable.Models;

namespace Dequeueable.Services.Queues
{
    internal sealed class QueueMessageExecutor(IQueueJob function) : IQueueMessageExecutor
    {
        public async Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            await function.ExecuteAsync(message, cancellationToken);
        }
    }
}
