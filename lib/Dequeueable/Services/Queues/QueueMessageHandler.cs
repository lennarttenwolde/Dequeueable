using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Models;
using Dequeueable.AzureQueueStorage.Services.Timers;
using Microsoft.Extensions.Logging;

namespace Dequeueable.AzureQueueStorage.Services.Queues
{
    internal sealed class QueueMessageHandler(IQueueMessageExecutor queueMessageExecutor,
        IQueueMessageManager queueMessageManager,
        TimeProvider timeProvider,
        ILogger<QueueMessageHandler> logger,
        IHostOptions options) : IQueueMessageHandler
    {
        internal TimeSpan MinimalVisibilityTimeoutDelay { get; set; } = TimeSpan.FromSeconds(15);

        public async Task HandleAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                await HandleMessageAsync(message, cancellationToken);
                logger.LogInformation("Executed message with id '{MessageId}' (Succeeded)", message.MessageId);
                await queueMessageManager.DeleteMessageAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while executing the queue message with id '{MessageId}'", message.MessageId);
                await HandleException(message, cancellationToken);
            }
        }

        private Task HandleMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource();
            var run = Task.Factory.StartNew(() => ExecuteMessageAsync(message, taskCompletionSource, cancellationToken), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

            return taskCompletionSource.Task;
        }

        private async Task ExecuteMessageAsync(Message message, TaskCompletionSource taskCompletionSource, CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await using var timer = new VisibilityTimeoutTimer(queueMessageManager, timeProvider, new LinearDelayStrategy(MinimalVisibilityTimeoutDelay));

            timer.Start(message, onFaultedAction: () =>
            {
                cts.Cancel();
                taskCompletionSource.TrySetException(new VisibilityTimeoutException($"Unable to update the visibilty timeout for message with id '{message.MessageId}'. Invisibility cannot be guaranteed."));
            });

            try
            {
                await queueMessageExecutor.ExecuteAsync(message, cts.Token);
                taskCompletionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
            }
        }

        private Task HandleException(Message message, CancellationToken cancellationToken)
        {
            return message.DequeueCount >= options.MaxDequeueCount
                ? queueMessageManager.MoveToPoisonQueueAsync(message, cancellationToken)
                : queueMessageManager.EnqueueMessageAsync(message, cancellationToken);
        }
    }
}
