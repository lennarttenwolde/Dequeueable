namespace Dequeueable.Services.DistributedLock
{
    internal interface IDistributedLockManager
    {
        Task<string> AquireLockAsync(string fileName, CancellationToken cancellationToken);
        Task ReleaseLockAsync(string leaseId, string fileName, CancellationToken cancellationToken);
        Task<DateTimeOffset> RenewLockAsync(string leaseId, string fileName, CancellationToken cancellationToken);
    }
}