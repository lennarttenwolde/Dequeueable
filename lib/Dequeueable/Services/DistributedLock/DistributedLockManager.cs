using Dequeueable.Configurations;
using Dequeueable.Factories;
using Dequeueable.Services.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dequeueable.Services.DistributedLock
{
    internal sealed class DistributedLockManager(ILogger<DistributedLockManager> logger,
    IBlobLeaseManagerFactory blobLeaseManagerFactory,
    IOptions<DistributedLockOptions> distributedLockOptions) : IDistributedLockManager
    {
        public async Task<string> AquireLockAsync(string fileName, CancellationToken cancellationToken)
        {

            var lockManager = blobLeaseManagerFactory.Create(fileName);

            var leaseId = await AcquireLockAsync(distributedLockOptions.Value, lockManager, cancellationToken);

            logger.LockAcquired(leaseId, fileName);

            return leaseId;
        }

        public async Task<DateTimeOffset> RenewLockAsync(string leaseId, string fileName, CancellationToken cancellationToken)
        {
            var lockManager = blobLeaseManagerFactory.Create(fileName);

            var nextVisibileOn = await lockManager.RenewAsync(leaseId, cancellationToken);

            logger.LockRenewed(leaseId);

            return nextVisibileOn;
        }

        public Task ReleaseLockAsync(string leaseId, string fileName, CancellationToken cancellationToken)
        {
            var lockManager = blobLeaseManagerFactory.Create(fileName);

            return lockManager.ReleaseAsync(leaseId, cancellationToken);
        }

        private static async Task<string> AcquireLockAsync(DistributedLockOptions singleton, IBlobLeaseManager leaseManager, CancellationToken cancellationToken)
        {
            var delayStrategy = new RandomizedExponentialDelayStrategy(TimeSpan.FromSeconds(singleton.MinimumPollingIntervalInSeconds), TimeSpan.FromSeconds(singleton.MaximumPollingIntervalInSeconds));

            for (var retry = 0; retry <= singleton.MaxRetries; retry++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var leaseId = await leaseManager.AcquireAsync(cancellationToken);

                if (leaseId is not null)
                {
                    return leaseId;
                }

                await Task.Delay(delayStrategy.GetNextDelay(executionSucceeded: false), cancellationToken);
            }

            throw new DistributedLockException($"Unable to acquire lock, max retries of '{singleton.MaxRetries}' reached");
        }
    }


    internal static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Lock with Id '{LeaseId}' acquired for '{FileName}'")]
        public static partial void LockAcquired(this ILogger logger, string leaseId, string fileName);

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Lock with Id '{LeaseId}' renewed")]
        public static partial void LockRenewed(this ILogger logger, string leaseId);
    }
}
