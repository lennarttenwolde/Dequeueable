using Azure.Storage.Blobs;
using Dequeueable.Configurations;
using Dequeueable.Services.DistributedLock;
using Microsoft.Extensions.Logging;

namespace Dequeueable.Factories
{
    internal sealed class BlobLeaseManagerFactory : IBlobLeaseManagerFactory
    {
        public IBlobLeaseManager Create(BlobClient blobClient, DistributedLockOptions options, ILogger logger)
        {
            return new BlobLeaseManager(blobClient, options, logger);
        }
    }
}
