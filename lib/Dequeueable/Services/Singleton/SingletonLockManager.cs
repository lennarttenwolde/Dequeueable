using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Factories;
using Dequeueable.AzureQueueStorage.Services.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dequeueable.AzureQueueStorage.Services.Singleton
{
    internal sealed class SingletonLockManager(ILogger<SingletonLockManager> logger,
    IBlobClientProvider blobClientProvider,
    IDistributedLockManagerFactory distributedLockManagerFactory,
    IOptions<SingletonHostOptions> singletonHostOptions) : ISingletonLockManager
    {
        public async Task<string> AquireLockAsync(string fileName, CancellationToken cancellationToken)
        {
            var blobClient = blobClientProvider.GetClient(fileName);
            var lockManager = distributedLockManagerFactory.Create(blobClient, singletonHostOptions.Value, logger);

            var leaseId = await AcquireLockAsync(singletonHostOptions.Value, lockManager, cancellationToken);

            logger.LogInformation("Lock with Id '{LeaseId}' acquired for '{FileName}'", leaseId, fileName);

            return leaseId;
        }

        public async Task<DateTimeOffset> RenewLockAsync(string leaseId, string fileName, CancellationToken cancellationToken)
        {
            var blobClient = blobClientProvider.GetClient(fileName);
            var lockManager = distributedLockManagerFactory.Create(blobClient, singletonHostOptions.Value, logger);

            var nextVisibileOn = await lockManager.RenewAsync(leaseId, cancellationToken);

            logger.LogInformation("Lock with Id '{LeaseId}' renewed", leaseId);
            return nextVisibileOn;
        }

        public Task ReleaseLockAsync(string leaseId, string fileName, CancellationToken cancellationToken)
        {
            var blobClient = blobClientProvider.GetClient(fileName);
            var lockManager = distributedLockManagerFactory.Create(blobClient, singletonHostOptions.Value, logger);

            return lockManager.ReleaseAsync(leaseId, cancellationToken);
        }

        private static async Task<string> AcquireLockAsync(SingletonHostOptions singleton, IDistributedLockManager lockManager, CancellationToken cancellationToken)
        {
            var delayStrategy = new RandomizedExponentialDelayStrategy(TimeSpan.FromSeconds(singleton.MinimumPollingIntervalInSeconds), TimeSpan.FromSeconds(singleton.MaximumPollingIntervalInSeconds));

            for (var retry = 0; retry <= singleton.MaxRetries; retry++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var leaseId = await lockManager.AcquireAsync(cancellationToken);

                if (leaseId is not null)
                {
                    return leaseId;
                }

                await Task.Delay(delayStrategy.GetNextDelay(executionSucceeded: false), cancellationToken);
            }

            throw new SingletonException($"Unable to acquire lock, max retries of '{singleton.MaxRetries}' reached");
        }
    }
}
