using Dequeueable.Models;

namespace Dequeueable.Services.Queues
{
    internal interface IQueueMessageExecutor
    {
        Task ExecuteAsync(Message message, CancellationToken cancellationToken);
    }
}
