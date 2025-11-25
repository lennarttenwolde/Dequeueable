using Dequeueable.AmazonSQS.Models;

namespace Dequeueable.AmazonSQS.Services.Queues
{
    internal sealed class QueueMessageExecutor(IAmazonSQSFunction function) : IQueueMessageExecutor
    {
        public Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            return function.ExecuteAsync(message, cancellationToken);
        }
    }
}
