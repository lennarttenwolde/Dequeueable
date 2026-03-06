using Azure.Storage.Blobs;
using Dequeueable.Configurations;
using Dequeueable.Services.Singleton;
using Microsoft.Extensions.Logging;

namespace Dequeueable.Factories
{
    internal interface IDistributedLockManagerFactory
    {
        IDistributedLockManager Create(BlobClient blobClient, SingletonHostOptions options, ILogger logger);
    }
}