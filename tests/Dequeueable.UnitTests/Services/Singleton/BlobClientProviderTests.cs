using Azure.Storage.Blobs;
using Azure.Identity;
using Dequeueable.Factories;
using Dequeueable.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Dequeueable.Services.DistributedLock;

namespace Dequeueable.UnitTests.Services.Singleton
{
    public class BlobClientProviderTests
    {
        [Fact]
        public void Given_a_BlobClientProvider_when_Get_is_called_with_a_ConnectionString_as_options_then_the_client_is_created_correctly()
        {
            // Arrange
            var fileName = "some-file";
            var options = new HostOptions { ConnectionString = "unit-test" };
            var distributedLockOptions = new DistributedLockOptions();
            var logger = new FakeLogger<BlobClientProvider>();
            var factoryMock = Substitute.For<IBlobClientFactory>();

            factoryMock.Create(options.ConnectionString, distributedLockOptions.ContainerName, fileName)
                .Returns(Substitute.For<BlobClient>());

            var sut = new BlobClientProvider(factoryMock, Options.Create(options), Options.Create(distributedLockOptions), logger);

            // Act
            sut.GetClient(fileName);

            // Assert
            factoryMock.Received(1).Create(options.ConnectionString, distributedLockOptions.ContainerName, fileName);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Debug && e.Message.Contains("Authenticate the BlobClient through the ConnectionString"));
        }

        [Fact]
        public void Given_a_BlobClientProvider_when_Get_is_called_with_an_AuthScheme_and_accountName_as_option_then_the_client_is_created_correctly()
        {
            // Arrange
            var fileName = "some-file";
            var options = new HostOptions
            {
                AuthenticationScheme = new DefaultAzureCredential(),
                AccountName = "testaccount"
            };
            var distributedLockOptions = new DistributedLockOptions();
            var logger = new FakeLogger<BlobClientProvider>();
            var factoryMock = Substitute.For<IBlobClientFactory>();

            factoryMock.Create(Arg.Is<Uri>(uri => uri.AbsoluteUri == "https://testaccount.blob.core.windows.net/webjobshost/some-file"), options.AuthenticationScheme)
                .Returns(Substitute.For<BlobClient>());

            var sut = new BlobClientProvider(factoryMock, Options.Create(options), Options.Create(distributedLockOptions), logger);

            // Act
            sut.GetClient(fileName);

            // Assert
            factoryMock.Received(1).Create(Arg.Is<Uri>(uri => uri.AbsoluteUri == "https://testaccount.blob.core.windows.net/webjobshost/some-file"), options.AuthenticationScheme);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Debug && e.Message.Contains("Authenticate the BlobClient through Active Directory"));
        }

        [Fact]
        public void Given_a_BlobClientProvider_when_Get_is_called_with_an_AuthScheme_and_different_UriFormat_as_option_then_the_client_is_created_correctly()
        {
            // Arrange
            var fileName = "some-file";
            var options = new HostOptions
            {
                AuthenticationScheme = new DefaultAzureCredential(),
                AccountName = "testaccount"
            };
            var distributedLockOptions = new DistributedLockOptions { BlobUriFormat = "https://{blobName}.privateazure.com" };
            var logger = new FakeLogger<BlobClientProvider>();
            var factoryMock = Substitute.For<IBlobClientFactory>();

            factoryMock.Create(Arg.Is<Uri>(uri => uri.AbsoluteUri == "https://some-file.privateazure.com/"), options.AuthenticationScheme)
                .Returns(Substitute.For<BlobClient>());

            var sut = new BlobClientProvider(factoryMock, Options.Create(options), Options.Create(distributedLockOptions), logger);

            // Act
            sut.GetClient(fileName);

            // Assert
            factoryMock.Received(1).Create(Arg.Is<Uri>(uri => uri.AbsoluteUri == "https://some-file.privateazure.com/"), options.AuthenticationScheme);
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Debug && e.Message.Contains("Authenticate the BlobClient through Active Directory"));
        }

        [Fact]
        public void Given_a_BlobClientProvider_when_Get_is_called_with_an_AuthScheme_and_an_invalid_AccountName_then_an_UriFormatException_is_thrown()
        {
            // Arrange
            var fileName = "some-file";
            var options = new HostOptions
            {
                AuthenticationScheme = new DefaultAzureCredential(),
                AccountName = "invalid account!"
            };
            var distributedLockOptions = new DistributedLockOptions();
            var logger = new FakeLogger<BlobClientProvider>();
            var factoryMock = Substitute.For<IBlobClientFactory>();

            var sut = new BlobClientProvider(factoryMock, Options.Create(options), Options.Create(distributedLockOptions), logger);

            // Act & Assert
            Assert.Throws<UriFormatException>(() => sut.GetClient(fileName));
            Assert.Contains(logger.Collector.GetSnapshot(), e => e.Level == LogLevel.Error && e.Message.Contains("Invalid Uri: The Blob Uri could not be parsed."));
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetQueue_is_called_with_no_AuthScheme_or_ConnectionString_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            var fileName = "some-file";
            var options = new HostOptions
            {
                AuthenticationScheme = null,
                ConnectionString = null
            };
            var distributedLockOptions = new DistributedLockOptions();
            var logger = new FakeLogger<BlobClientProvider>();
            var factoryMock = Substitute.For<IBlobClientFactory>();

            var sut = new BlobClientProvider(factoryMock, Options.Create(options), Options.Create(distributedLockOptions), logger);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => sut.GetClient(fileName));

            // Assert
            Assert.Equal("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings", ex.Message);
        }
    }
}