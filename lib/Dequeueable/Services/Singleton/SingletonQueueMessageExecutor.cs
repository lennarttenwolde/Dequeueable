using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Extentions;
using Dequeueable.AzureQueueStorage.Models;
using Dequeueable.AzureQueueStorage.Services.Queues;
using Dequeueable.AzureQueueStorage.Services.Timers;
using Microsoft.Extensions.Options;

namespace Dequeueable.AzureQueueStorage.Services.Singleton
{
    internal sealed class SingletonQueueMessageExecutor(ISingletonLockManager singletonLockManager,
        IQueueMessageExecutor queueMessageExecutor,
        TimeProvider timeProvider,
        IOptions<SingletonHostOptions> singletonHostOptions) : IQueueMessageExecutor
    {
        private readonly SingletonHostOptions _options = singletonHostOptions.Value;

        public async Task ExecuteAsync(Message message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.Scope))
            {
                throw new InvalidOperationException("The Singleton Scope cannot be empty when creating a scoped distributed lock");
            }

            await ExecuteMessageAsync(message, cancellationToken);
        }

        private Task ExecuteMessageAsync(Message message, CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource();
            var run = Task.Factory.StartNew(() => ExecuteMessageAsync(message, taskCompletionSource, cancellationToken), CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

            return taskCompletionSource.Task;
        }

        private async Task ExecuteMessageAsync(Message message, TaskCompletionSource taskCompletionSource, CancellationToken cancellationToken)
        {
            string lockName;
            string leaseId;
            try
            {
                lockName = GetLockName(message);
                leaseId = await singletonLockManager.AquireLockAsync(lockName, cancellationToken);
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
                return;
            }

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                await using var timer = new LeaseTimeoutTimer(singletonLockManager, timeProvider, new LinearDelayStrategy(TimeSpan.FromSeconds(_options.MinimumPollingIntervalInSeconds)));

                timer.Start(leaseId, lockName, onFaultedAction: () =>
                {
                    taskCompletionSource.TrySetException(new SingletonException($"Unable to renew the lease with id '{leaseId}'. Distributed lock cannot be guaranteed."));
                    cts.Cancel();
                });

                await queueMessageExecutor.ExecuteAsync(message, cts.Token);
                taskCompletionSource.TrySetResult();
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
            }
            finally
            {
                await singletonLockManager.ReleaseLockAsync(leaseId, lockName, cancellationToken);
            }
        }

        private string GetLockName(Message message)
        {
            try
            {
                var lockName = message.GetValueByPropertyName(_options.Scope);

                return string.IsNullOrWhiteSpace(lockName)
                    ? throw new SingletonException($"The provided scope name, '{_options.Scope}' , does not exist on the message with id '{message.MessageId}'")
                    : lockName;
            }
            catch (KeyNotFoundException ex)
            {
                throw new SingletonException($"The provided scope name, '{_options.Scope}' , does not exist on the message with id '{message.MessageId}'", ex);
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new SingletonException($"Unable to parse the body for the message with id '{message.MessageId}'", ex);
            }
        }
    }
}
