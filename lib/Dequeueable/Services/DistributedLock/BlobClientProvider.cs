using Azure.Storage.Blobs;
using Dequeueable.Configurations;
using Dequeueable.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dequeueable.Services.DistributedLock
{
    internal sealed class BlobClientProvider(
    IBlobClientFactory factory,
    IOptions<HostOptions> hostOptions,
    IOptions<DistributedLockOptions> distributedLockOptions,
    ILogger<BlobClientProvider> logger) : IBlobClientProvider
    {

        private readonly HostOptions _hostOptions = hostOptions.Value;
        private readonly DistributedLockOptions _distributedLockOptions = distributedLockOptions.Value;


        public BlobClient GetClient(string fileName)
        {
            if (_hostOptions.AuthenticationScheme is not null)
            {
                logger.LogDebug("Authenticate the BlobClient through Active Directory");

                var uri = BuildUri(_distributedLockOptions.BlobUriFormat, _hostOptions.AccountName, _distributedLockOptions.ContainerName, fileName);
                return factory.Create(uri, _hostOptions.AuthenticationScheme);
            }

            if (string.IsNullOrWhiteSpace(_hostOptions.ConnectionString))
            {
                throw new InvalidOperationException("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings");
            }

            logger.LogDebug("Authenticate the BlobClient through the ConnectionString");
            return factory.Create(_hostOptions.ConnectionString, _distributedLockOptions.ContainerName, fileName);
        }

        private Uri BuildUri(string? uriFormat, string? accountName, string containerName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(uriFormat))
            {
                throw new ArgumentException($"'{nameof(uriFormat)}' cannot be null or whitespace.", nameof(uriFormat));
            }

            if (string.IsNullOrWhiteSpace(accountName) == false)
            {
                uriFormat = uriFormat.Replace($"{{{nameof(HostOptions.AccountName)}}}", accountName, StringComparison.InvariantCultureIgnoreCase);
            }

            uriFormat = uriFormat.Replace($"{{{nameof(_distributedLockOptions.ContainerName)}}}", containerName, StringComparison.InvariantCultureIgnoreCase);
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
