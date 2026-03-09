using Azure.Storage.Blobs;
using Dequeueable.Configurations;
using Dequeueable.Services.DistributedLock;
using Microsoft.Extensions.Logging;

namespace Dequeueable.Factories
{
    internal interface IBlobLeaseManagerFactory
    {
        IBlobLeaseManager Create(BlobClient blobClient, DistributedLockOptions options, ILogger logger);
    }
}