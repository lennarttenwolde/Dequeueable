using Azure.Storage.Blobs;
using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Services.Singleton;
using Microsoft.Extensions.Logging;

namespace Dequeueable.AzureQueueStorage.Factories
{
    internal interface IDistributedLockManagerFactory
    {
        IDistributedLockManager Create(BlobClient blobClient, SingletonHostOptions options, ILogger logger);
    }
}