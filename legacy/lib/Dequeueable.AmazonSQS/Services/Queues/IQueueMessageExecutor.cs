using Dequeueable.AmazonSQS.Models;

namespace Dequeueable.AmazonSQS.Services.Queues
{
    internal interface IQueueMessageExecutor
    {
        Task ExecuteAsync(Message message, CancellationToken cancellationToken);
    }
}
