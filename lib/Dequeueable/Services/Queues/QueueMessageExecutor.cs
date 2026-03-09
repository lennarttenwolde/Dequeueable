using Dequeueable.Models;

namespace Dequeueable.Services.Queues
{
    internal sealed class QueueMessageExecutor(IQueueJob function) : IQueueMessageExecutor
    {
        public Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            return function.ExecuteAsync(message, cancellationToken);
        }
    }
}
