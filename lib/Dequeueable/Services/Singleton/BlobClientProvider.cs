using Azure.Storage.Blobs;
using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dequeueable.AzureQueueStorage.Services.Singleton
{
    internal sealed class BlobClientProvider(
        IBlobClientFactory factory,
        IHostOptions options,
        IOptions<SingletonHostOptions> singletonHostOptions,
        ILogger<BlobClientProvider> logger) : IBlobClientProvider
    {
        private readonly SingletonHostOptions _singletonHostOptions = singletonHostOptions.Value;

        public BlobClient GetClient(string fileName)
        {
            if (options.AuthenticationScheme is not null)
            {
                logger.LogDebug("Authenticate the BlobClient through Active Directory");

                var uri = BuildUri(_singletonHostOptions.BlobUriFormat, options.AccountName, _singletonHostOptions.ContainerName, fileName);
                return factory.Create(uri, options.AuthenticationScheme);
            }

            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                throw new InvalidOperationException("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings");
            }

            logger.LogDebug("Authenticate the BlobClient through the ConnectionString");
            return factory.Create(options.ConnectionString, _singletonHostOptions.ContainerName, fileName);
        }

        private Uri BuildUri(string? uriFormat, string? accountName, string containerName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(uriFormat))
            {
                throw new ArgumentException($"'{nameof(uriFormat)}' cannot be null or whitespace.", nameof(uriFormat));
            }

            if (string.IsNullOrWhiteSpace(accountName) == false)
            {
                uriFormat = uriFormat.Replace($"{{{nameof(IHostOptions.AccountName)}}}", accountName, StringComparison.InvariantCultureIgnoreCase);
            }

            uriFormat = uriFormat.Replace($"{{{nameof(_singletonHostOptions.ContainerName)}}}", containerName, StringComparison.InvariantCultureIgnoreCase);
            uriFormat = uriFormat.Replace($"{{blobName}}", fileName, StringComparison.InvariantCultureIgnoreCase);

            try
            {
                return new Uri(uriFormat);
            }
            catch (UriFormatException)
            {
                logger.LogError("Invalid Uri: The Blob Uri could not be parsed. Format: '{Uri}'", uriFormat);
                throw;
            }
        }
    }
}
