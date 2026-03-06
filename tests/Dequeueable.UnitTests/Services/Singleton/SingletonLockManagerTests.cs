using Azure.Storage.Blobs;
using Dequeueable.AzureQueueStorage.Configurations;
using Dequeueable.AzureQueueStorage.Factories;
using Dequeueable.AzureQueueStorage.Services.Singleton;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Dequeueable.AzureQueueStorage.UnitTests.Services.Singleton
{
    public class SingletonLockManagerTests
    {
        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_is_acquired_then_the_leaseId_is_returned()
        {
            // Arrange
            var leaseId = "someId";
            var fileName = "someName";
            var options = new HostOptions { ConnectionString = "some string" };
            var loggerMock = new Mock<ILogger<SingletonLockManager>>(MockBehavior.Strict);

            var blobClientProviderMock = new Mock<IBlobClientProvider>(MockBehavior.Strict);
            var distributedLockManagerFactoryMock = new Mock<IDistributedLockManagerFactory>(MockBehavior.Strict);
            var distributedLockManagerMock = new Mock<IDistributedLockManager>(MockBehavior.Strict);
            var singletonHostOptions = new SingletonHostOptions();
            var singletonHostOptionsMock = new Mock<IOptions<SingletonHostOptions>>();
            var blobClientFake = new Mock<BlobClient>();

            singletonHostOptionsMock.Setup(o => o.Value).Returns(singletonHostOptions);
            blobClientProviderMock.Setup(c => c.GetClient(fileName)).Returns(blobClientFake.Object);
            distributedLockManagerFactoryMock.Setup(f => f.Create(blobClientFake.Object, singletonHostOptions, loggerMock.Object)).Returns(distributedLockManagerMock.Object);
            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Lock with Id '{leaseId}' acquired for '{fileName}'")),
                null,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
                .Verifiable();
            distributedLockManagerMock.Setup(m => m.AcquireAsync(CancellationToken.None)).ReturnsAsync(leaseId);

            var sut = new SingletonLockManager(loggerMock.Object, blobClientProviderMock.Object, distributedLockManagerFactoryMock.Object, singletonHostOptionsMock.Object);

            // Act
            var result = await sut.AquireLockAsync(fileName, CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_cannot_be_acquired_at_first_then_it_is_retried_correctly()
        {
            // Arrange
            var leaseId = "someId";
            var fileName = "someName";
            var options = new HostOptions { ConnectionString = "some string" };
            var loggerMock = new Mock<ILogger<SingletonLockManager>>(MockBehavior.Strict);

            var blobClientProviderMock = new Mock<IBlobClientProvider>(MockBehavior.Strict);
            var distributedLockManagerFactoryMock = new Mock<IDistributedLockManagerFactory>(MockBehavior.Strict);
            var distributedLockManagerMock = new Mock<IDistributedLockManager>(MockBehavior.Strict);
            var singletonHostOptions = new SingletonHostOptions { MaxRetries = 5, MinimumPollingIntervalInSeconds = 1, MaximumPollingIntervalInSeconds = 1 };
            var singletonHostOptionsMock = new Mock<IOptions<SingletonHostOptions>>();
            var blobClientFake = new Mock<BlobClient>();

            singletonHostOptionsMock.Setup(o => o.Value).Returns(singletonHostOptions);
            blobClientProviderMock.Setup(c => c.GetClient(fileName)).Returns(blobClientFake.Object);
            distributedLockManagerFactoryMock.Setup(f => f.Create(blobClientFake.Object, singletonHostOptions, loggerMock.Object)).Returns(distributedLockManagerMock.Object);
            loggerMock.Setup(
                x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Lock with Id '{leaseId}' acquired for '{fileName}'")),
                null,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)))
                .Verifiable();
            distributedLockManagerMock.SetupSequence(m => m.AcquireAsync(CancellationToken.None))
                .ReturnsAsync((string?)null)
                .ReturnsAsync(leaseId);

            var sut = new SingletonLockManager(loggerMock.Object, blobClientProviderMock.Object, distributedLockManagerFactoryMock.Object, singletonHostOptionsMock.Object);

            // Act
            var result = await sut.AquireLockAsync(fileName, CancellationToken.None);

            // Assert
            result.Should().Be(leaseId);
        }

        [Fact]
        public async Task Given_a_LockManager_when_AquireLockAsync_is_called_and_the_lock_cannot_be_acquired_and_the_MaxRetries_is_reached_then_a_SingletonException_is_thrown()
        {
            // Arrange
            var fileName = "someName";
            var options = new HostOptions { ConnectionString = "some string" };
            var loggerMock = new Mock<ILogger<SingletonLockManager>>(MockBehavior.Strict);

            var blobClientProviderMock = new Mock<IBlobClientProvider>(MockBehavior.Strict);
            var distributedLockManagerFactoryMock = new Mock<IDistributedLockManagerFactory>(MockBehavior.Strict);
            var distributedLockManagerMock = new Mock<IDistributedLockManager>(MockBehavior.Strict);
            var singletonHostOptions = new SingletonHostOptions { MaxRetries = 1, MinimumPollingIntervalInSeconds = 1, MaximumPollingIntervalInSeconds = 1 };
            var singletonHostOptionsMock = new Mock<IOptions<SingletonHostOptions>>();
            var blobClientFake = new Mock<BlobClient>();

            singletonHostOptionsMock.Setup(o => o.Value).Returns(singletonHostOptions);
            blobClientProviderMock.Setup(c => c.GetClient(fileName)).Returns(blobClientFake.Object);
            distributedLockManagerFactoryMock.Setup(f => f.Create(blobClientFake.Object, singletonHostOptions, loggerMock.Object)).Returns(distributedLockManagerMock.Object);
            distributedLockManagerMock.SetupSequence(m => m.AcquireAsync(CancellationToken.None))
                .ReturnsAsync((string?)null)
                .ReturnsAsync((string?)null);

            var sut = new SingletonLockManager(loggerMock.Object, blobClientProviderMock.Object, distributedLockManagerFactoryMock.Object, singletonHostOptionsMock.Object);

            // Act
            Func<Task> act = () => sut.AquireLockAsync(fileName, CancellationToken.None);

            // Assert
            await act.Should().ThrowExactlyAsync<SingletonException>().WithMessage($"Unable to acquire lock, max retries of '{singletonHostOptions.MaxRetries}' reached");
        }
    }
}
