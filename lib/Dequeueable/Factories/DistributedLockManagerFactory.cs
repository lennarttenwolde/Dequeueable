using Azure.Storage.Blobs;
using Dequeueable.Configurations;
using Dequeueable.Services.Singleton;
using Microsoft.Extensions.Logging;

namespace Dequeueable.Factories
{
    internal sealed class DistributedLockManagerFactory : IDistributedLockManagerFactory
    {
        public IDistributedLockManager Create(BlobClient blobClient, SingletonHostOptions options, ILogger logger)
        {
            return new DistributedLockManager(blobClient, options, logger);
        }
    }
}
