using Moq;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Dequeueable.Factories;
using Dequeueable.Configurations;
using Microsoft.Extensions.Options;
using Azure.Identity;
using FluentAssertions;
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
            var options = new HostOptions
            {
                ConnectionString = "unit-test",
            };

            var distributedLockOptions = new DistributedLockOptions();
            var singletonHostOptionsMock = new Mock<IOptions<DistributedLockOptions>>();
            var loggerMock = new Mock<ILogger<BlobClientProvider>>();
            var factoryMock = new Mock<IBlobClientFactory>(MockBehavior.Strict);

            singletonHostOptionsMock.Setup(o => o.Value).Returns(distributedLockOptions);
            factoryMock.Setup(f => f.Create(options.ConnectionString, distributedLockOptions.ContainerName, fileName))
                .Returns(new Mock<BlobClient>().Object)
                .Verifiable();

            var sut = new BlobClientProvider(factoryMock.Object, Options.Create(options), singletonHostOptionsMock.Object, loggerMock.Object);

            // Act
            sut.GetClient(fileName);

            // Assert
            factoryMock.Verify();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authenticate the BlobClient through the ConnectionString")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var singletonHostOptionsMock = new Mock<IOptions<DistributedLockOptions>>();
            var loggerMock = new Mock<ILogger<BlobClientProvider>>();
            var factoryMock = new Mock<IBlobClientFactory>(MockBehavior.Strict);

            singletonHostOptionsMock.Setup(o => o.Value).Returns(distributedLockOptions);

            factoryMock.Setup(f => f.Create(It.Is<Uri>(uri => uri.AbsoluteUri == "https://testaccount.blob.core.windows.net/webjobshost/some-file"), options.AuthenticationScheme))
                .Returns(new Mock<BlobClient>().Object)
                .Verifiable();

            var sut = new BlobClientProvider(factoryMock.Object, Options.Create(options), singletonHostOptionsMock.Object, loggerMock.Object);

            // Act
            sut.GetClient(fileName);

            // Assert
            factoryMock.Verify();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authenticate the BlobClient through Active Directory")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var singletonHostOptionsMock = new Mock<IOptions<DistributedLockOptions>>();
            var loggerMock = new Mock<ILogger<BlobClientProvider>>();
            var factoryMock = new Mock<IBlobClientFactory>(MockBehavior.Strict);

            singletonHostOptionsMock.Setup(o => o.Value).Returns(distributedLockOptions);

            factoryMock.Setup(f => f.Create(It.Is<Uri>(uri => uri.AbsoluteUri == "https://some-file.privateazure.com/"), options.AuthenticationScheme))
                .Returns(new Mock<BlobClient>().Object)
                .Verifiable();

            var sut = new BlobClientProvider(factoryMock.Object, Options.Create(options), singletonHostOptionsMock.Object, loggerMock.Object);

            // Act
            sut.GetClient(fileName);

            // Assert
            factoryMock.Verify();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Authenticate the BlobClient through Active Directory")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
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
            var singletonHostOptionsMock = new Mock<IOptions<DistributedLockOptions>>();
            var loggerMock = new Mock<ILogger<BlobClientProvider>>();
            var factoryMock = new Mock<IBlobClientFactory>(MockBehavior.Strict);

            singletonHostOptionsMock.Setup(o => o.Value).Returns(distributedLockOptions);

            var sut = new BlobClientProvider(factoryMock.Object, Options.Create(options), singletonHostOptionsMock.Object, loggerMock.Object);

            // Act
            Action act = () => sut.GetClient(fileName);

            // Assert
            act.Should().ThrowExactly<UriFormatException>();
            loggerMock.Verify(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Invalid Uri: The Blob Uri could not be parsed.")),
                It.IsAny<UriFormatException>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
        }

        [Fact]
        public void Given_a_QueueClientProvider_when_GetQueue_is_called_with_no_AuthScheme_or_ConnectionString_then_an_InvalidOperationException_is_thrown()
        {
            // Arrange
            // Arrange
            var fileName = "some-file";
            var options = new HostOptions
            {
                AuthenticationScheme = null,
                ConnectionString = null
            };

            var distributedLockOptions = new DistributedLockOptions();
            var singletonHostOptionsMock = new Mock<IOptions<DistributedLockOptions>>();
            var loggerMock = new Mock<ILogger<BlobClientProvider>>();
            var factoryMock = new Mock<IBlobClientFactory>(MockBehavior.Strict);

            singletonHostOptionsMock.Setup(o => o.Value).Returns(distributedLockOptions);

            var sut = new BlobClientProvider(factoryMock.Object, Options.Create(options), singletonHostOptionsMock.Object, loggerMock.Object);

            // Act
            Action act = () => sut.GetClient(fileName);

            // Assert
            act.Should().ThrowExactly<InvalidOperationException>().WithMessage("No AuthenticationScheme or ConnectionString supplied. Make sure that it is defined in the app settings");
        }
    }
}
