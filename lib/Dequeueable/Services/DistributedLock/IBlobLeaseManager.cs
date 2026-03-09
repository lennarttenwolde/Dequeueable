namespace Dequeueable.Services.DistributedLock
{
    internal interface IBlobLeaseManager
    {
        Task<string?> AcquireAsync(CancellationToken cancellationToken);
        Task ReleaseAsync(string leaseId, CancellationToken cancellationToken);
        Task<DateTimeOffset> RenewAsync(string leaseId, CancellationToken cancellationToken);
    }
}