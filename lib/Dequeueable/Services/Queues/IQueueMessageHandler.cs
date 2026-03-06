using Dequeueable.Models;

namespace Dequeueable.Services.Queues
{
    internal interface IQueueMessageHandler
    {
        Task HandleAsync(Message message, CancellationToken cancellationToken);
    }
}
