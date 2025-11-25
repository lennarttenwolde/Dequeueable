using Dequeueable.AmazonSQS.Models;

namespace Dequeueable.AmazonSQS.Services.Queues
{
    internal sealed class SingletonQueueMessageExecutor(IQueueMessageExecutor executor, SingletonManager singletonManager) : IQueueMessageExecutor
    {
        public async Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(message.MessageGroupId))
            {
                await executor.ExecuteAsync(message, cancellationToken);
                return;
            }

            await singletonManager.WaitAsync(message.MessageGroupId, cancellationToken);
            try
            {
                await executor.ExecuteAsync(message, cancellationToken);
            }
            finally
            {
                singletonManager.Release(message.MessageGroupId);
            }
        }
    }
}
