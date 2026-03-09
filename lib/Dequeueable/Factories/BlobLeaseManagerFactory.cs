
using Dequeueable.Configurations;
using Dequeueable.Services.DistributedLock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dequeueable.Factories
{
    internal sealed class BlobLeaseManagerFactory(
    IBlobClientProvider blobClientProvider,
    IOptions<DistributedLockOptions> options,
    ILogger<BlobLeaseManager> logger) : IBlobLeaseManagerFactory
    {
        public IBlobLeaseManager Create(string fileName)
            => new BlobLeaseManager(fileName, blobClientProvider, options.Value, logger);
    }
}
